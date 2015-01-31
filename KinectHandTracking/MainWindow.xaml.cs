using System;
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
        private CoordinateMapper coordinateMapper = null;
        int attackState = 0;
        int attackChange = 0;
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

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
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
                                string rightHandState = "-";
                                string leftHandState = "-";


                                Ellipse rhandellip = new Ellipse
                                {
                                    Width = 40,
                                    Height = 40,
                                    Fill = new SolidColorBrush(Colors.LightBlue),
                                    Opacity = 0.7
                                };

                                Ellipse headellip = new Ellipse
                                {
                                    Width = 800,
                                    Height = 800,
                                    Fill = new SolidColorBrush(Colors.Orange),
                                    Opacity = 0.7
                                };

                                switch (body.HandRightState)
                                {
                                    case HandState.Open:
                                        rightHandState = "Open";
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
                                        rightHandState = "Closed";
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
                                        rightHandState = "Lasso";
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
                                        rightHandState = "Unknown...";
                                        attackState = 0;
                                        attackChange = 0;
                                        break;
                                    case HandState.NotTracked:
                                        rightHandState = "Not tracked";
                                        break;
                                    default:
                                        break;
                                }

                                switch (body.HandLeftState)
                                {
                                    case HandState.Open:
                                        leftHandState = "Open";
                                        break;
                                    case HandState.Closed:
                                        leftHandState = "Closed";
                                        break;
                                    case HandState.Lasso:
                                        leftHandState = "Lasso";
                                        break;
                                    case HandState.Unknown:
                                        leftHandState = "Unknown...";
                                        break;
                                    case HandState.NotTracked:
                                        leftHandState = "Not tracked";
                                        break;
                                    default:
                                        break;
                                }

                                if (attackChange==1)
                                {
                                    if (attackState == 1)
                                    {
                                        //Deal damage
                                    }
                                    if (attackState == 2)
                                    {
                                        //Block
                                    }
                                    if (attackState == 3)
                                    {
                                        //Spell
                                    }
                                }

                                double fx = handRight.Position.X;
                                double fy = handRight.Position.Y;
                                //tblRightHandState.Text = rightHandState + "\n" + fx.ToString() + "\n" + fy.ToString();
                                //tblLeftHandState.Text = leftHandState;

                                Canvas.SetLeft(rhandellip, fx*1000+900 - rhandellip.Width / 2);
                                Canvas.SetTop(rhandellip, -fy*1000+800 - rhandellip.Height / 2);

                                canvas.Children.Add(rhandellip);

                                var headJoint = body.Joints[JointType.Head].Position;

                                CameraSpacePoint pt = new CameraSpacePoint()
                                {
                                    X = headJoint.X,
                                    Y = headJoint.Y,
                                    Z = headJoint.Z
                                };
                                ColorSpacePoint clpt = this.coordinateMapper.MapCameraPointToColorSpace(pt);
                                Rectangle headbox = new Rectangle
                                {
                                    Width = 80,
                                    Height = 80,
                                    Fill = new SolidColorBrush(Colors.Orange),
                                    Opacity = 0.7
                                };

                                Rectangle healthbar = new Rectangle
                                {
                                    Width = 400,
                                    Height = 40,
                                    Fill = new SolidColorBrush(Colors.Red),
                                    Opacity = 0.7
                                };



                                Canvas.SetLeft(headbox, clpt.X- headbox.Width / 2);
                                Canvas.SetTop(headbox, clpt.Y- headbox.Height / 2);
                                Canvas.SetLeft(healthbar, clpt.X - healthbar.Width / 2);
                                Canvas.SetTop(healthbar, clpt.Y - 100 - headbox.Height / 2);

                                canvas.Children.Add(headbox);
                                canvas.Children.Add(healthbar);

                                //healthbar.Width = 100;

                                /*
                                FaceFrameSource[] faceFrameSources = null;
                                FaceFrameReader[] faceFrameReaders = null;
                                FaceFrameResult[] faceFrameResults = null;

                                faceFrameSources = new FaceFrameSource[_bodies.Count];
                                faceFrameReaders = new FaceFrameReader[_bodies.Count];
                                for (int i = 0; i < _bodies.Count; i++)
                                {
                                    // create the face frame source with the required face frame features and an initial tracking Id of 0
                                    faceFrameSources[i] = new FaceFrameSource(this._sensor, 0, FaceFrameFeatures.BoundingBoxInColorSpace);

                                    // open the corresponding reader
                                    faceFrameReaders[i] = faceFrameSources[i].OpenReader();
                                }
                                
                                // allocate storage to store face frame results for each face in the FOV
                                faceFrameResults = new FaceFrameResult[_bodies.Count];
                                var faceBoxSource = faceFrameResults[0].FaceBoundingBoxInColorSpace;
                                Rectangle fb = new Rectangle
                                {
                                    Width = faceBoxSource.Right - faceBoxSource.Left,
                                    Height = faceBoxSource.Bottom - faceBoxSource.Top,
                                    Fill = new SolidColorBrush(Colors.Orange),
                                    Opacity = 0.7
                                };
                                Canvas.SetLeft(fb, faceBoxSource.Left - fb.Width / 2);
                                Canvas.SetTop(fb, faceBoxSource.Top - fb.Height / 2);

                                canvas.Children.Add(fb);*/
                                

                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
