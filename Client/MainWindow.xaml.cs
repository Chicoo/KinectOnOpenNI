using System;
using System.Collections.Generic;
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
using KinectOnOpenNI;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.Dispatcher tabDispatcher;
        public MainWindow()
        {
            InitializeComponent();
            _manager = new OpenNIManager();

            this.Loaded += (sender, args) => InitTracker();
        }

        //private GestureHandler _gestures;
        private OpenNIManager _manager;

        private void InitTracker()
        {
            
            _manager.EnableVideo = true;
            _manager.VideoUpdated += OnTracker_UpdateViewPort;
            _manager.VideoStopped += OnTracker_TrackinkgCompleted;
            _manager.StartVideo();

            _manager.EnableGestures = true;
            _manager.GestureTrackingStarted += new EventHandler<EventArgs>(_manager_GestureTrackingStarted);
            _manager.StillDetected += new EventHandler<StillGestureRecognisedEventArgs>(_manager_StillDetected);
            _manager.SwipeDetected += new EventHandler<SwipeGestureRecognisedEventArgs>(_manager_SwipeDetected);
            _manager.PushDetected += new EventHandler<PushGestureRecognisedEventArgs>(_manager_PushDetected);
            _manager.AddGesture(Gestures.Push);
            _manager.AddGesture(Gestures.SwipeAll);
            _manager.AddGesture(Gestures.Still);
            _manager.StartGestureTracking();

            //_gestures = new GestureHandler();
            //_gestures.SessionStarted += new EventHandler<SessionStartedEventArgs>(_gestures_SessionStarted);
            //_gestures.CircleDetected += new EventHandler<CircleDetectedEventArgs>(_gestures_CircleDetected);
            //_gestures.PushDetected += new EventHandler<PushEventArgs>(_gestures_PushDetected);
            //_gestures.SwipeDetected += new EventHandler<SwipeEventArgs>(_gestures_SwipeDetected);
            //_gestures.SteadyDetected += new EventHandler<SteadyDetectEventArgs>(_gestures_SteadyDetected);
            //_gestures.Init();
        }

        void _manager_PushDetected(object sender, PushGestureRecognisedEventArgs e)
        {
            string text = "State: Push Detected";
            System.Windows.Threading.Dispatcher pdDispatcher = lblState.Dispatcher;
            //create a new delegate for updating our progress text            
            UpdateStatusTextDelegate update = new UpdateStatusTextDelegate(UpdateStatusText);
            //invoke the dispatcher and pass the percentage and max record count            
            pdDispatcher.BeginInvoke(update, text);
            
        }

        void _manager_SwipeDetected(object sender, SwipeGestureRecognisedEventArgs e)
        {
            string text = "State: Swipe Detected of type" + e.DetectedGesture;
            System.Windows.Threading.Dispatcher pdDispatcher = lblState.Dispatcher;
            //create a new delegate for updating our progress text            
            UpdateStatusTextDelegate update = new UpdateStatusTextDelegate(UpdateStatusText);
            //invoke the dispatcher and pass the percentage and max record count            
            pdDispatcher.BeginInvoke(update, text);

            tabDispatcher = tabControl1.Dispatcher;
            UpdateTabControlDelegate updateTab = new UpdateTabControlDelegate(UpdateTabControl);
            tabDispatcher.BeginInvoke(updateTab, e.DetectedGesture);
        }

        void _manager_StillDetected(object sender, StillGestureRecognisedEventArgs e)
        {
            string text = "State: Still hand Detected";
            System.Windows.Threading.Dispatcher pdDispatcher = lblState.Dispatcher;
            //create a new delegate for updating our progress text            
            UpdateStatusTextDelegate update = new UpdateStatusTextDelegate(UpdateStatusText);
            //invoke the dispatcher and pass the percentage and max record count            
            pdDispatcher.BeginInvoke(update, text);
        }

        void _manager_ClickDetected(object sender, EventArgs e)
        {
            string text = "State: Click Detected";
            System.Windows.Threading.Dispatcher pdDispatcher = lblState.Dispatcher;
            //create a new delegate for updating our progress text            
            UpdateStatusTextDelegate update = new UpdateStatusTextDelegate(UpdateStatusText);
            //invoke the dispatcher and pass the percentage and max record count            
            pdDispatcher.BeginInvoke(update, text);
        }

        void _manager_GestureTrackingStarted(object sender, EventArgs e)
        {
            string text = "State: Session Started";
            System.Windows.Threading.Dispatcher pdDispatcher = lblState.Dispatcher;
            //create a new delegate for updating our progress text            
            UpdateStatusTextDelegate update = new UpdateStatusTextDelegate(UpdateStatusText);
            //invoke the dispatcher and pass the percentage and max record count            
            pdDispatcher.BeginInvoke(update, text);
        }

        //our delegate used for updating the UI
        public delegate void UpdateStatusTextDelegate(string text);
        private void UpdateStatusText(string text)
        {
            lblState.Content = text;
        }

        //our delegate used for updating the UI
        public delegate void UpdateTabControlDelegate(Gestures gesture);
        private void UpdateTabControl(Gestures gesture)
        {
            //Get the new tabindex
            int tabIndex = 0;
            if (tabControl1.SelectedIndex == -1) tabIndex = 0;
            else
            {
                switch (gesture)
                {
                    case Gestures.SwipeLeft:
                        if (tabControl1.SelectedIndex != 0) tabIndex = tabControl1.SelectedIndex - 1;
                        else tabIndex = tabControl1.Items.Count - 1;
                        break;
                    case Gestures.SwipeRight:
                        if (tabControl1.SelectedIndex != tabControl1.Items.Count - 1) tabIndex = tabControl1.SelectedIndex + 1;
                        else tabIndex = 0;
                        break;
                }
            }
            tabControl1.SelectedIndex = tabIndex;
        }

        void OnTracker_TrackinkgCompleted(object sender, EventArgs e)
        {
            //image.Source = null;
            depth.Source = null;
            //scene.Source = null;
        }

        void OnTracker_UpdateViewPort(object sender, EventArgs e)
        {
            //image.Source = _videoTracker.RgbImageSource;
            depth.Source = _manager.DepthImage;
            //scene.Source = _videoTracker.SceneImageSource;
            //fpsText.Text = _videoTracker.FramesPerSecond.ToString("F1");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_manager != null) _manager.Shutdown();
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) _manager.TiltUp();
            if (e.Delta < 0) _manager.TiltDown();
        }
    }
}
