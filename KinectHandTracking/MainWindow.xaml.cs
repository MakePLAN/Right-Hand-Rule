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
        Dictionary<ulong, int> charge;
        Dictionary<ulong, int> atkState;
        private CoordinateMapper coordinateMapper = null;
        int attackState = 0;
        int attackChange = 0;
        int damage = 0;
        int newchlcount = 0;
        int count1 = 0;
        int gameState = 1;
        ColorSpacePoint clpt1;
        double saveWidth = 0;
        double saveHeight = 0; 
        

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
            charge = new Dictionary<ulong, int>();
            atkState = new Dictionary<ulong, int>();

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
                        double fx = 0, fy = 0, hx = 0, hy = 0;
                        if (body != null && body.TrackingId != 0)
                        {
                            

                            Ellipse rhandellip = new Ellipse
                            {
                                Width = 40,
                                Height = 40,
                                Fill = new SolidColorBrush(Colors.Red),
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
                                string rightHandState = "-";
                                string leftHandState = "-";


                                Ellipse headellip = new Ellipse
                                {
                                    Width = 800,
                                    Height = 800,
                                    Fill = new SolidColorBrush(Colors.Orange),
                                    Opacity = 0.7
                                };

                                // hx = handRight.Position.X * 1000 + 900;
                                hy = -handRight.Position.Y * 1000 + 800;
                                fx = handRight.Position.X;
                                hx = 500 + fx * 3000;
                                fy = handRight.Position.Y;

                                //tblRightHandState.Text = rightHandState + "\n" + fx.ToString() + "\n" + fy.ToString();
                                //tblLeftHandState.Text = leftHandState;

                                switch (body.HandRightState)
                                {
                                    case HandState.Open:
                                        rhandellip.Fill = new SolidColorBrush(Colors.Red);
                                        atkState[body.TrackingId] = 1;

                                        break;
                                    case HandState.Closed:
                                        rhandellip.Fill = new SolidColorBrush(Colors.Green);
                                        atkState[body.TrackingId] = 2;
                                        break;
                                    case HandState.Lasso:
                                        rhandellip.Fill = new SolidColorBrush(Colors.Yellow);
                                        atkState[body.TrackingId] = 3;
                                        break;
                                    case HandState.Unknown:
                                        atkState[body.TrackingId] = 0;
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
                                    newchlcount = 0;
                                    
                                }

                                try
                                {
                                    int a = atkState[body.TrackingId];
                                }
                                catch
                                {
                                    atkState.Add(body.TrackingId, 0);
                                }

                                try
                                {
                                    int a = charge[body.TrackingId];
                                }
                                catch
                                {
                                    charge.Add(body.TrackingId, 0);
                                }

                                /*try
                                {
                                    int a = atkChange[body.TrackingId];
                                }
                                catch
                                {
                                    atkChange.Add(body.TrackingId, 0);
                                }*/

                                //if (atkChange[body.TrackingId] == 1)
                                //{

                                //}
                            }

                            var headJoint = body.Joints[JointType.Head].Position;

                            CameraSpacePoint pt = new CameraSpacePoint()
                            {
                                X = headJoint.X,
                                Y = headJoint.Y,
                                Z = headJoint.Z
                            };
                            ColorSpacePoint clpt = this.coordinateMapper.MapCameraPointToColorSpace(pt);
                            clpt1 = this.coordinateMapper.MapCameraPointToColorSpace(pt); 
                            facex[i] = clpt.X;
                            facey[i] = clpt.Y;
                            fx -= headJoint.X;
                            fy -= headJoint.Y;
                            Rectangle headbox = new Rectangle
                            {
                                Width = 100,
                                Height = 100,
                                //Fill = new SolidColorBrush(Colors.Orange),
                                Opacity = 0
                            };
                            saveWidth = headbox.Width;
                            saveHeight = headbox.Height;
                            Rectangle healthbar = new Rectangle
                            {
                                Width = health[body.TrackingId],
                                //Width = 100,
                                Height = 40,
                                Fill = new SolidColorBrush(Colors.Green),
                                Opacity = 0.7
                            };
                            
                            Rectangle chargebar = new Rectangle
                            {
                                    Width = charge[body.TrackingId],
                                    //Width = 100,
                                    Height = 40,
                                    Fill = new SolidColorBrush(Colors.Yellow),
                                    Opacity = 0.7
                                
                            };
                            
                            charge[body.TrackingId] += 3;
                            if (charge[body.TrackingId] > 400)
                                charge[body.TrackingId] = 400;
                            if (atkState[body.TrackingId] == 1)
                            {
                                for (int j = 0; j < _bodies.Count; j++)
                                {
                                    if (((facex[j] < (40 + 600 + fx * 2000)) && ((facex[j] + 100) > (600 + fx * 2000))) && ((facey[j] < (40 + hy - 200)) && ((facey[j] + 100) > (hy - 200))))
                                    {
                                        try
                                        {
                                            if (atkState[_bodies[j].TrackingId] == 2)
                                            {
                                                health[_bodies[j].TrackingId] += 20;
                                            }
                                            else
                                            {
                                                health[_bodies[j].TrackingId] -= 5;
                                                if (health[_bodies[j].TrackingId] <= 0)
                                                {
                                                    health[_bodies[j].TrackingId] = 0;
                                                }
                                            }

                                        }
                                        catch
                                        {
                                            try {
                                                health.Add(_bodies[j].TrackingId, 370);
                                            }
                                            catch
                                            {

                                            }
                                        }
                                    }
                                }
                            }
                            if (atkState[body.TrackingId] == 2)
                            {
                                health[body.TrackingId] -= 1;
                                if (health[body.TrackingId] < 0)
                                {
                                    health[body.TrackingId] = 0;
                                }
                            }
                            if (atkState[body.TrackingId] == 3)
                            {
                                for (int j = 0; j < _bodies.Count; j++)
                                {
                                    if (((facex[j] < (40 + 600 + fx * 2000)) && ((facex[j] + 100) > (600 + fx * 2000))) && ((facey[j] < (40 + hy - 200)) && ((facey[j] + 100) > (hy-200))))
                                    {
                                        try
                                        {
                                            if (atkState[_bodies[j].TrackingId] == 2)
                                            {
                                                health[_bodies[j].TrackingId] -= charge[body.TrackingId] / 8;
                                                charge[body.TrackingId] = 0;
                                            }
                                            health[_bodies[j].TrackingId] -= charge[body.TrackingId]/2;
                                            if (health[_bodies[j].TrackingId] <= 0)
                                            {
                                                health[_bodies[j].TrackingId] = 0;
                                            }
                                            charge[body.TrackingId] = 0;
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                charge.Add(_bodies[j].TrackingId, 0);
                                            }
                                            catch
                                            {

                                            }
                                        }
                                    }
                                    charge[body.TrackingId] /= 2;
                                }
                                //charge[body.TrackingId] = 0;
                            }
                            string Testing = "right hand";
                            string Facetesting = "Face";
                           // tblRightHandState.Text = Testing + "\n" + (500 + fx * 3000).ToString() + "\n" + hy.ToString();
                            //tblLeftHandState.Text = Facetesting + "\n" + (clpt.X - headbox.Width / 2).ToString() + "\n" + (clpt.Y - headbox.Height / 2).ToString();
                            try
                            {
                                Canvas.SetLeft(rhandellip, 600 + fx * 2000);
                                Canvas.SetTop(rhandellip, hy - 200);
                                canvas.Children.Add(rhandellip);
                                Canvas.SetLeft(headbox, clpt.X - headbox.Width / 2);
                                Canvas.SetTop(headbox, clpt.Y - headbox.Height / 2);
                                canvas.Children.Add(headbox);
                                Canvas.SetLeft(healthbar, clpt.X - healthbar.Width / 2);
                                Canvas.SetTop(healthbar, clpt.Y - 100 - headbox.Height / 2);
                                canvas.Children.Add(healthbar);
                                Canvas.SetLeft(chargebar, clpt.X - healthbar.Width / 2);
                                Canvas.SetTop(chargebar, clpt.Y - 55 - headbox.Height / 2);
                                canvas.Children.Add(chargebar);

                                if (healthbar.Width == 0)
                                {

                                    BitmapImage image = new BitmapImage();
                                    image.BeginInit();
                                    image.UriSource = new Uri("bomb.jpg", UriKind.RelativeOrAbsolute);
                                    image.EndInit();
                                    ImageBrush myImageBrush = new ImageBrush(image);

                                    Canvas myCanvas = new Canvas();
                                    myCanvas.Width = 300;
                                    myCanvas.Height = 300;
                                    myCanvas.Background = myImageBrush;
                                    Canvas.SetLeft(myCanvas, clpt.X - headbox.Width / 2 - (myCanvas.Width/2));
                                    Canvas.SetTop(myCanvas, clpt.Y - headbox.Height / 2 - (myCanvas.Width/ 2));
                                    canvas.Children.Add(myCanvas);
                                    chargebar.Width = 0;
                                    
                                }
                                
                            }
                            catch
                            {

                            }

                        }
                    }
                    

                    if (gameState == 1)
                    {
                        if (count1 < 100)
                        {
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri("start.jpg", UriKind.RelativeOrAbsolute);
                            image.EndInit();
                            ImageBrush myImageBrush = new ImageBrush(image);

                            Canvas myCanvas = new Canvas();
                            myCanvas.Width = 1000;
                            myCanvas.Height = 200;
                            myCanvas.Background = myImageBrush;
                            Canvas.SetLeft(myCanvas, 500);
                            Canvas.SetTop(myCanvas, 700);
                            canvas.Children.Add(myCanvas);
                            count1++;
                            newchlcount = 0;
                            if (newchlcount < 50)
                            {
                                BitmapImage image1 = new BitmapImage();
                                image1.BeginInit();
                                image1.UriSource = new Uri("troll.png", UriKind.RelativeOrAbsolute);
                                image1.EndInit();
                                ImageBrush myImageBrush1 = new ImageBrush(image1);
                                
                                Canvas myCanvas1 = new Canvas();
                                myCanvas1.Width = 150;
                                myCanvas1.Height = 150;
                                myCanvas1.Background = myImageBrush1;
                                Canvas.SetLeft(myCanvas1, clpt1.X - this.saveWidth / 2 - (myCanvas1.Width / 2));
                                Canvas.SetTop(myCanvas1, clpt1.Y - this.saveHeight / 2 -(myCanvas1.Width / 2));
                                canvas.Children.Add(myCanvas1);
                                newchlcount++;
                            }
                        }
                        else
                            gameState = 0;
                    }
                    else
                    {
                        if (newchlcount < 100 && gameState == 0)
                        {
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri("challenge.png", UriKind.RelativeOrAbsolute);
                            image.EndInit();
                            ImageBrush myImageBrush = new ImageBrush(image);

                            Canvas myCanvas = new Canvas();
                            myCanvas.Width = 1000;
                            myCanvas.Height = 200;
                            myCanvas.Background = myImageBrush;
                            Canvas.SetLeft(myCanvas, 500);
                            Canvas.SetTop(myCanvas, 700);
                            canvas.Children.Add(myCanvas);
                            newchlcount++;

                            int newchlcount1 = 0;
                            if (newchlcount1 < 50)
                            {
                                BitmapImage image1 = new BitmapImage();
                                image1.BeginInit();
                                image1.UriSource = new Uri("troll.png", UriKind.RelativeOrAbsolute);
                                image1.EndInit();
                                ImageBrush myImageBrush1 = new ImageBrush(image1);

                                Canvas myCanvas1 = new Canvas();
                                myCanvas1.Width = 150;
                                myCanvas1.Height = 150;
                                myCanvas1.Background = myImageBrush1;
                                Canvas.SetLeft(myCanvas1, clpt1.X - this.saveWidth / 2 - (myCanvas1.Width / 2));
                                Canvas.SetTop(myCanvas1, clpt1.Y - this.saveHeight / 2 - (myCanvas1.Width / 2));
                                canvas.Children.Add(myCanvas1);
                                newchlcount1++;
                            }

                        }
                    }
                }
            }
        }

        #endregion
    }
}
