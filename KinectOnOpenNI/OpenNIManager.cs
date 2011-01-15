using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xn;
using System.Windows.Media;

namespace KinectOnOpenNI
{
    /// <summary>
    /// Handles the control over the OpenNI copntrols.
    /// All getsures and controls are handled through this class.
    /// </summary>
    public class OpenNIManager
    {
        #region Variables
        private Context _context;
        private KinectMotor _motor;
        #endregion Variables

        #region Manager events
        /// <summary>
        /// Fired when the manager is deactivated
        /// </summary>
        public event EventHandler<EventArgs> Deactivated;
        #endregion Manager events

        #region Properties
        /// <summary>
        /// The OpenNI context in which the manager operates. 
        /// </summary>
        public Context ONIContext
        {
            get
            {
                return _context;
            }
        }
        #endregion Properties

        #region Constructors
        /// <summary>
        /// Instanciates a new manager.
        /// </summary>
        public OpenNIManager()
            : this("openni.xml")
        {
        }

        /// <summary>
        /// Instanciates a new manager with a configuration file
        /// </summary>
        public OpenNIManager(string filename)
        {
            _context = new Context(filename);
            _motor = new KinectMotor();
        }
        #endregion Constructors

        #region Public Manager methods
        /// <summary>
        /// Shut down the manager and dispose all created OpenNI objects.
        /// </summary>
        public void Shutdown()
        {
            Deactivated(this, new EventArgs());
            _context.Dispose();
        }
        #endregion Public Manager methods

        #region Internal Methods
        internal xn.Point3D ConvertProjectiveToRealWorld(xn.Point3D projective)
        {
            return _videoTracker._depthNode.ConvertProjectiveToRealWorld(projective);
        }
        #endregion Internal Methods

        #region Video Tracking
        private OpenNIVideo _videoTracker;
        private bool _enableVideo = false;
        private bool _videoTrackingOn = false;

        /// <summary>
        /// Triggered when the video image is updated.
        /// </summary>
        public event EventHandler<EventArgs> VideoUpdated;
        /// <summary>
        /// Triggered when the video tracking is stopped.
        /// </summary>
        public event EventHandler<EventArgs> VideoStopped;

        /// <summary>
        /// Gets/sets the indication that video is enabled.
        /// If true video can be used.
        /// </summary>
        public bool EnableVideo
        {
            get { return _enableVideo; }
            set 
            {
                //If current setting is false and the new value is true, create the video tracker
                if (!_enableVideo && value)
                {
                    _videoTracker = new OpenNIVideo(this);
                    _videoTracker.TrackinkgCompleted += new EventHandler(_videoTracker_TrackingCompleted);
                    _videoTracker.UpdateViewPort += new EventHandler(_videoTracker_UpdateViewPort);
                }
                _enableVideo = value; 
            }
        }

        /// <summary>
        /// Gets the color image from the video
        /// </summary>
        public ImageSource RgbImage
        {
            get 
            {
                if (!_enableVideo && !_videoTrackingOn)
                    throw new Exception("Video should be enabled and started.");
                return _videoTracker.RgbImageSource;
            }
        }

        /// <summary>
        /// Gets the depth image from the video
        /// </summary>
        public ImageSource DepthImage
        {
            get
            {
                if (!_enableVideo && !_videoTrackingOn)
                    throw new Exception("Video should be enabled and started.");
                return _videoTracker.DepthImageSource;
            }
        }

        /// <summary>
        /// Start the tracking of video images
        /// </summary>
        public void StartVideo()
        {
            //If video is not enabled throw an error.
            if (!_enableVideo)
                throw new Exception("Video cannot be started when EnableVideo is false");
            if (!_videoTrackingOn)
            {
                _videoTrackingOn = true;
                _videoTracker.StartTracking();
            }
        }

        /// <summary>
        /// Stops the video tracking.
        /// </summary>
        public void StopVideo()
        {
            if (_videoTrackingOn)
            {
                _videoTrackingOn = false;
                _videoTracker.StopTracking();
            }
        }

        void _videoTracker_UpdateViewPort(object sender, EventArgs e)
        {
            VideoUpdated(this, e);
        }

        void _videoTracker_TrackingCompleted(object sender, EventArgs e)
        {
            VideoStopped(this, e);
        }
        #endregion Video Tracking

        #region Gesture Tracking
        private KinectGestureTracker _gestureTracker;
        private bool _enableGestures = false;
        private bool _gestureTrackingOn = false;
        public List<Gestures> _allowedGestures = new List<Gestures>();

        /// <summary>
        /// Fired when the iniating gesture is recognised.
        /// This means that next gestures will be handled.
        /// </summary>
        public event EventHandler<EventArgs> GestureTrackingStarted;
        /// <summary>
        /// Fired when a still gesture is detected.
        /// </summary>
        public event EventHandler<StillGestureRecognisedEventArgs> StillDetected;
        /// <summary>
        /// Fired when a swipe gesture is detected.
        /// </summary>
        public event EventHandler<SwipeGestureRecognisedEventArgs> SwipeDetected;
        /// <summary>
        /// Fired when a push gesture is detected.
        /// </summary>
        public event EventHandler<PushGestureRecognisedEventArgs> PushDetected;

        /// <summary>
        /// Returns the gestures that are tracked
        /// </summary>
        public IEnumerable<Gestures> AllowedGestures
        {
            get { return _allowedGestures.AsReadOnly(); }
        }

        /// <summary>
        /// Gets/sets the enabling of gesture tracking
        /// </summary>
        public bool EnableGestures
        {
            get { return _enableGestures; }
            set
            {
                //If current setting is false and the new value is true, create the video tracker
                if (!_enableGestures && value)
                {
                    _gestureTracker = new KinectGestureTracker(this);
                    _gestureTracker.SessionStateChanged +=new EventHandler<SessionStateChangedEventArgs>(_gestureTracker_SessionStateChanged);
                    _gestureTracker.GestureDetected += new EventHandler<GestureRecognisedEventArgs>(_gestureTracker_GestureDetected);
                    _gestureTracker.StillGestureRecognized += new EventHandler<StillGestureRecognisedEventArgs>(_gestureTracker_StillGestureRecognized);
                    _gestureTracker.SwipeGestureRecognized += new EventHandler<SwipeGestureRecognisedEventArgs>(_gestureTracker_SwipeGestureRecognized);
                    _gestureTracker.PushGestureRecognized += new EventHandler<PushGestureRecognisedEventArgs>(_gestureTracker_PushGestureRecognized);
                }
                _enableGestures = value;
            }
        }

       /// <summary>
        /// Add a gesture to the list of tracked gestures.
        /// </summary>
        /// <param name="gesture">The gesture to add.</param>
        public void AddGesture(Gestures gesture)
        {
            if (!_allowedGestures.Contains(gesture))
            {
                _allowedGestures.Add(gesture);
                if (_gestureTracker != null) _gestureTracker.AddGesture(gesture);
            }
        }

        /// <summary>
        /// Removes a gesture from the list of tracked gestures.
        /// </summary>
        /// <param name="gesture">The gesture to remove</param>
        public void RemoveGesture(Gestures gesture)
        {
            if (_allowedGestures.Contains(gesture))
            {
                _allowedGestures.Remove(gesture);
            }
        }

        /// <summary>
        /// Starts the tracking of gestures.
        /// </summary>
        public void StartGestureTracking()
        {
            //If video is not enabled throw an error.
            if (!_enableGestures)
                throw new Exception("Gesture tracking cannot be started when EnableGestures is false");
            if (!_gestureTrackingOn)
            {
                _gestureTrackingOn = true;
                _gestureTracker.StartGestures();
                SetLed(KinectLEDStatus.AlternateRedGreen);
            }
        }

        void _gestureTracker_SessionStateChanged(object sender, SessionStateChangedEventArgs e)
        {
            switch (e.NewSessionState)
            {
                case SessionState.InProgress:
                    SetLed(KinectLEDStatus.Green);
                    break;
                case SessionState.Starting:
                    GestureTrackingStarted(this, e);
                    SetLed(KinectLEDStatus.Yellow);
                    break;
                case SessionState.Halted:
                    SetLed(KinectLEDStatus.Red);
                    break;
                case SessionState.Stopped:
                    SetLed(KinectLEDStatus.AlternateRedGreen);
                    break;
            }
        }

        void _gestureTracker_StillGestureRecognized(object sender, StillGestureRecognisedEventArgs e)
        {
            StillDetected(this, e);
        }

        void _gestureTracker_SwipeGestureRecognized(object sender, SwipeGestureRecognisedEventArgs e)
        {
            SwipeDetected(this, e);
        }

        void _gestureTracker_PushGestureRecognized(object sender, PushGestureRecognisedEventArgs e)
        {
            PushDetected(this, e);
        }

        void _gestureTracker_GestureDetected(object sender, GestureRecognisedEventArgs e)
        {
           
        }

        #endregion Gesture Tracking

        #region MouseTracking
        #endregion MouseTracking

        #region KinectMotor
        /// <summary>
        /// Gets a value that indicates if the motor is operational
        /// </summary>
        public bool CanOperate
        {
            get { return _motor.CanOperate; }
        }

        /// <summary>
        /// Tilt the Kinect up.
        /// If the maximum tilt is reached the motor will stop.
        /// </summary>
        public void TiltUp()
        {
            _motor.TiltUp();
        }

        /// <summary>
        /// Tilt the Kinect down.
        /// If the minimum tilt is reached the motor will stop.
        /// </summary>
        public void TiltDown()
        {
            _motor.TitlDown();
        }

        internal void SetLed(KinectLEDStatus ledStatus)
        {
            _motor.SetLED(ledStatus);
        }
        #endregion
    }
}
