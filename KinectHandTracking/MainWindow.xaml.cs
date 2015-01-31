﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;

namespace KinectHandTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;
        Dictionary<ulong, int> health;
        private CoordinateMapper coordinateMapper = null;
        int attackState = 0;
        int attackChange = 0;
        int damage = 0;

        IList<double> facex;
        IList<double> facey;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();
            facex = new double[10];
            facey = new double[10];
            health = new Dictionary<ulong, int>();
            for (int i = 0; i < 10; i++)
            {
                facex[i] = 0;
                facey[i] = 0;
            }

            this.coordinateMapper = _sensor.CoordinateMapper;
            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);
                    for (int i = 0; i < _bodies.Count; i++)
                    {
                        var body = _bodies[i];
                        double fx=0, fy=0, hx=0, hy=0;
                        if (body != null && body.TrackingId != 0)
                        {

                            Ellipse rhandellip = new Ellipse
                            {
                                Width = 40,
                                Height = 40,
                                Fill = new SolidColorBrush(Colors.LightBlue),
                                Opacity = 0.7
                            };
                            if (body.IsTracked)
                            {
                                // Find the joints
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];

                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];


                                // Draw hands and thumbs
                                /*canvas.DrawHand(handRight, _sensor.CoordinateMapper);
                                canvas.DrawHand(handLeft, _sensor.CoordinateMapper);
                                canvas.DrawThumb(thumbRight, _sensor.CoordinateMapper);
                                canvas.DrawThumb(thumbLeft, _sensor.CoordinateMapper);*/

                                // Find the hand states
                                //string rightHandState = "-";
                                //string leftHandState = "-";


                                Ellipse headellip = new Ellipse
                                {
                                    Width = 800,
                                    Height = 800,
                                    Fill = new SolidColorBrush(Colors.Orange),
                                    Opacity = 0.7
                                };

                                hx = handRight.Position.X * 1000 + 900;
                                hy = -handRight.Position.Y * 1000 + 800;
                                fx = handRight.Position.X;
                                fy = handRight.Position.Y;

                                //tblRightHandState.Text = rightHandState + "\n" + fx.ToString() + "\n" + fy.ToString();
                                //tblLeftHandState.Text = leftHandState;

                                switch (body.HandRightState)
                                {
                                    case HandState.Open:
                                        rhandellip.Fill = new SolidColorBrush(Colors.Red);
                                        if (attackState == 1)
                                        {
                                            attackChange = 0;
                                        }
                                        else
                                        {
                                            attackState = 1;
                                            attackChange = 1;
                                        }
                                        break;
                                    case HandState.Closed:
                                        rhandellip.Fill = new SolidColorBrush(Colors.Green);
                                        if (attackState == 2)
                                        {
                                            attackChange = 0;
                                        }
                                        else
                                        {
                                            attackState = 2;
                                            attackChange = 1;
                                        }
                                        break;
                                    case HandState.Lasso:
                                        rhandellip.Fill = new SolidColorBrush(Colors.Yellow);
                                        if (attackState == 3)
                                        {
                                            attackChange = 0;
                                        }
                                        else
                                        {
                                            attackState = 3;
                                            attackChange = 1;
                                        }
                                        break;
                                    case HandState.Unknown:
                                        attackState = 0;
                                        attackChange = 0;
                                        break;
                                    case HandState.NotTracked:
                                        break;
                                    default:
                                        break;
                                }
                                /*
                                switch (body.HandLeftState)
                                {
                                    case HandState.Open:
                                        break;
                                    case HandState.Closed:
                                        break;
                                    case HandState.Lasso:
                                        break;
                                    case HandState.Unknown:
                                        break;
                                    case HandState.NotTracked:
                                        break;
                                    default:
                                        break;
                                }*/

                                try
                                {
                                    int a = health[body.TrackingId];
                                }
                                catch
                                {
                                    health.Add(body.TrackingId, 400);
                                }

                                if (attackChange == 1)
                                {
                                    if (attackState == 1)
                                    {
                                        if ( /*face!=ball*/ attackState != 2)
                                            //////{
                                            for (int j = 0; j < _bodies.Count; j++)
                                            {
                                                if (((facex[j] < (40 + hx)) || ((facex[j] + 100) > hx)) && ((facey[j] < (40 + hy)) || ((facey[j] + 100) > hy)))
                                                {
                                                    try
                                                    {
                                                        health[_bodies[j].TrackingId] -= 30;
                                                        if (health[_bodies[j].TrackingId] <= 0)
                                                        {
                                                            health[_bodies[j].TrackingId] = 0;
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        health.Add(_bodies[j].TrackingId, 370);
                                                    }
                                                }
                                            }
                                    }
                                }
                                if (attackState == 2)
                                {


                                }
                                if (attackState == 3)
                                {
                                    //Spell
                                }
                            }


                            var headJoint = body.Joints[JointType.Head].Position;

                            CameraSpacePoint pt = new CameraSpacePoint()
                            {
                                X = headJoint.X,
                                Y = headJoint.Y,
                                Z = headJoint.Z
                            };
                            ColorSpacePoint clpt = this.coordinateMapper.MapCameraPointToColorSpace(pt);
                            facex[i] = clpt.X;
                            facey[i] = clpt.Y;
                            fx -= headJoint.X;
                            fy -= headJoint.Y;
                            Rectangle headbox = new Rectangle
                            {
                                Width = 100,
                                Height = 100,
                                Fill = new SolidColorBrush(Colors.Orange),
                                Opacity = 0.7
                            };
                            Rectangle healthbar = new Rectangle
                            {
                                Width = health[body.TrackingId],
                                //Width = 100,
                                Height = 40,
                                Fill = new SolidColorBrush(Colors.Red),
                                Opacity = 0.7
                            };
                            try
                            {
                                Canvas.SetLeft(rhandellip, 500+fx*3000);
                                Canvas.SetTop(rhandellip, hy);
                                //Canvas.SetLeft(rhandellip, 1000);
                                //Canvas.SetTop(rhandellip, 500);
                                canvas.Children.Add(rhandellip);
                                Canvas.SetLeft(headbox, clpt.X - headbox.Width / 2);
                                Canvas.SetTop(headbox, clpt.Y - headbox.Height / 2);
                                canvas.Children.Add(headbox);
                                Canvas.SetLeft(healthbar, clpt.X - healthbar.Width / 2);
                                Canvas.SetTop(healthbar, clpt.Y - 100 - headbox.Height / 2);
                                canvas.Children.Add(healthbar);
                            }
                            catch
                            {

                            }

                        }
                    }
                }
            }
        }

        #endregion
    }
}
