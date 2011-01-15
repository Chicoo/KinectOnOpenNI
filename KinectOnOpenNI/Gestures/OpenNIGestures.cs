using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xn;

namespace KinectOnOpenNI
{
   
    internal class KinectGestureTracker
    {
        #region Private Variables
        private GestureGenerator _startSessionGenerator;
        private GestureGenerator _gestureGenerator;
        private OpenNIManager _manager;
        List<HandGesture> _handGestures;
        private bool sessionStarted = false;
        private SessionState _state = SessionState.Stopped;
        #endregion Private Variables

        #region Properties
        internal SessionState State
        {
            get
            {
                return _state;
            }
            set
            {
                if (value != _state)
                {
                    SessionState oldState = _state;
                    _state = value;
                    SessionStateChanged(this, new SessionStateChangedEventArgs(_state, oldState));
                }
            }
        }
        #endregion Properties

        #region Events
        internal event EventHandler<GestureRecognisedEventArgs> GestureDetected;
        internal event EventHandler<SessionStateChangedEventArgs> SessionStateChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Gestures Handler
        /// </summary>
        /// <param name="manager">The manager that controls the handler</param>
        internal KinectGestureTracker(OpenNIManager manager)
        {
            //Assign the manager
            _manager = manager;
            manager.Deactivated += new EventHandler<EventArgs>(manager_Deactivated);
            //Add the wave recognizer as this will start a session.
            //_startSessionGenerator = manager.ONIContext.FindExistingNode(NodeType.Gesture) as GestureGenerator;
            _startSessionGenerator = new GestureGenerator(manager.ONIContext);
            _startSessionGenerator.AddGesture("Wave");
            _startSessionGenerator.AddGesture("Click");
            
            _startSessionGenerator.GestureRecognized += new GestureGenerator.GestureRecognizedHandler(_startSessionGenerator_GestureRecognized);
            _startSessionGenerator.GestureChanged += new StateChangedHandler(_startSessionGenerator_GestureChanged);
            _startSessionGenerator.GestureProgress += new GestureGenerator.GestureProgressHandler(_startSessionGenerator_GestureProgress);
            _startSessionGenerator.GenerationRunningChanged += new StateChangedHandler(_startSessionGenerator_GenerationRunningChanged);
            _startSessionGenerator.NewDataAvailable += new StateChangedHandler(_startSessionGenerator_NewDataAvailable);

            //The gesture generator will detect gestures after the session has been activated.
            _gestureGenerator = new GestureGenerator(manager.ONIContext);
            _gestureGenerator.GestureRecognized += new GestureGenerator.GestureRecognizedHandler(_gestureGenerator_GestureRecognized);
            _gestureGenerator.GestureChanged += new StateChangedHandler(_gestureGenerator_GestureChanged);
            _gestureGenerator.GestureProgress += new GestureGenerator.GestureProgressHandler(_gestureGenerator_GestureProgress);
            _gestureGenerator.GenerationRunningChanged += new StateChangedHandler(_gestureGenerator_GenerationRunningChanged);
            _gestureGenerator.NewDataAvailable += new StateChangedHandler(_gestureGenerator_NewDataAvailable);

            _handGestures = new List<HandGesture>();
        }
        #endregion Constructors

        #region Gesture generator
        void _gestureGenerator_NewDataAvailable(ProductionNode node)
        {
            //throw new NotImplementedException();
        }

        void _gestureGenerator_GenerationRunningChanged(ProductionNode node)
        {
            //throw new NotImplementedException();
        }

        void _gestureGenerator_GestureProgress(ProductionNode node, string strGesture, ref xn.Point3D position, float progress)
        {
            //throw new NotImplementedException();
        }

        void _gestureGenerator_GestureChanged(ProductionNode node)
        {
            //throw new NotImplementedException();
        }

        void _gestureGenerator_GestureRecognized(ProductionNode node, string strGesture, ref xn.Point3D idPosition, ref xn.Point3D endPosition)
        {
            Gestures gesture;
            if(Enum.TryParse<Gestures>(strGesture, out gesture))
            {
                Point3D point = new Point3D(idPosition.X, idPosition.Y, idPosition.Z);
                GestureDetected(this, new GestureRecognisedEventArgs(gesture, point));
            }
        }

        /// <summary>
        /// Adds a gesture to recognize
        /// </summary>
        /// <param name="gesture">The gesture to recognize</param>
        internal void AddGesture(Gestures gesture)
        {
            if (_gestureGenerator != null)
            {
                switch (gesture)
                {
                    case Gestures.Push:
                        PushGesture pg = new PushGesture();
                        _handGestures.Add(pg);
                        pg.PushGestureRecognisedEventHandler += new EventHandler<PushGestureRecognisedEventArgs>(pg_PushGestureRecognisedEventHandler);
                        pg.HandDestroyed += new EventHandler<EventArgs>(hand_HandDestroyed);
                        pg.Init(_manager);
                        break;
                    case Gestures.SwipeAll:
                        SwipeGesture sg = new SwipeGesture();
                        _handGestures.Add(sg);
                        sg.SwipeGestureRecognisedEventHandler += new EventHandler<SwipeGestureRecognisedEventArgs>(sg_SwipeGestureRecognisedEventHandler);
                        sg.HandDestroyed += new EventHandler<EventArgs>(hand_HandDestroyed);
                        sg.Init(_manager);
                        break;
                    case Gestures.Still:
                        StillGesture stillg = new StillGesture();
                        _handGestures.Add(stillg);
                        stillg.StillGestureRecognized += new EventHandler<StillGestureRecognisedEventArgs>(stillg_StillGestureRecognized);
                        stillg.HandDestroyed += new EventHandler<EventArgs>(hand_HandDestroyed);
                        stillg.Init(_manager);
                        break;

                }
            }
        }
        
        /// <summary>
        /// Removes a gesture to recognize
        /// </summary>
        /// <param name="gesture">The gesture to remove</param>
        internal void RemoveGesture(Gestures gesture)
        {
            if (_gestureGenerator != null)
            {
                switch (gesture)
                {
                    case Gestures.Push:
                        PushGesture pg = _handGestures.Find(hg => hg.GetType() == typeof(PushGesture)) as PushGesture;
                        if (pg != null)
                        {
                            _handGestures.Remove(pg);
                            pg.Dispose();
                            pg = null;
                        }
                        break;
                    case Gestures.SwipeAll:
                        SwipeGesture sg = _handGestures.Find(hg => hg.GetType() == typeof(SwipeGesture)) as SwipeGesture;
                        if (sg != null)
                        {
                            _handGestures.Remove(sg);
                            sg.Dispose();
                            sg = null;
                        }
                        break;
                    case Gestures.Still:
                        StillGesture stillg = _handGestures.Find(hg => hg.GetType() == typeof(StillGesture)) as StillGesture;
                        if (stillg != null)
                        {
                            _handGestures.Remove(stillg);
                            stillg.Dispose();
                            stillg = null;
                        }
                        break;
                }
            }
        }
        #endregion Gesture generator

        #region StartSession Events
        void _startSessionGenerator_NewDataAvailable(ProductionNode node)
        {
            //throw new NotImplementedException();
        }

        void _startSessionGenerator_GenerationRunningChanged(ProductionNode node)
        {
            //throw new NotImplementedException();
        }

        void _startSessionGenerator_GestureProgress(ProductionNode node, string strGesture, ref xn.Point3D position, float progress)
        {
            System.Diagnostics.Debug.WriteLine("Gesture Progress:" + progress);
            //throw new NotImplementedException();
        }

        void _startSessionGenerator_GestureChanged(ProductionNode node)
        {
            System.Diagnostics.Debug.WriteLine("Gesture Changed");
            //throw new NotImplementedException();
        }

        void _startSessionGenerator_GestureRecognized(ProductionNode node, string strGesture, ref xn.Point3D idPosition, ref xn.Point3D endPosition)
        {
            System.Diagnostics.Debug.WriteLine("Gesture Recognized:" + strGesture);
            switch (strGesture)
            {
                case "Wave":
                case "Click":
                    if (State == SessionState.Stopped)
                    {
                        
                        State = SessionState.Starting;
                        _startSessionGenerator.RemoveGesture("Wave");
                        _startSessionGenerator.RemoveGesture("Click");
                        _startSessionGenerator.AddGesture("RaiseHand");
                        _gestureGenerator.StartGenerating();
                    }
                    break;
                case "RaiseHand":
                    if (State == SessionState.Halted || State == SessionState.Starting)
                    {
                        System.Diagnostics.Debug.WriteLine("Raised hand");
                        foreach (var hg in _handGestures)
                        {
                            if(!hg.IsTracking) hg.StartTracking(idPosition);
                        }
                        State = SessionState.InProgress;
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion StartSession Events

        #region Manager Events
        //Called when the manager object is shutdown.
        void manager_Deactivated(object sender, EventArgs e)
        {
            StopGestures();
            //Dispose the wave generator
            if (_startSessionGenerator != null) _startSessionGenerator.Dispose();
            foreach(var hg in _handGestures)
            {
                hg.Dispose();
                //hg = null;
            }

        }
        #endregion Manager Events

        #region Gesture Methods
        internal void StartGestures()
        {
            if (_startSessionGenerator != null) _startSessionGenerator.StartGenerating();
        }

        internal void StopGestures()
        {
            if (_startSessionGenerator != null) _startSessionGenerator.StopGenerating();
        }
        #endregion Gesture Methods

        #region Gesture events
        internal event EventHandler<StillGestureRecognisedEventArgs> StillGestureRecognized;
        internal event EventHandler<SwipeGestureRecognisedEventArgs> SwipeGestureRecognized;
        internal event EventHandler<PushGestureRecognisedEventArgs> PushGestureRecognized;
        #endregion Gesture events

        #region Gesture EventHandlers
        void hand_HandDestroyed(object sender, EventArgs e)
        {
            //Check if all hands have stopped tracking then no tracking is done and send event
            foreach (var hg in _handGestures)
            {
                if (hg.IsTracking) return;
            }
            State = SessionState.Halted;

        }

        void pg_PushGestureRecognisedEventHandler(object sender, PushGestureRecognisedEventArgs e)
        {
            PushGestureRecognized(this, e);
        }

        void stillg_StillGestureRecognized(object sender, StillGestureRecognisedEventArgs e)
        {
            StillGestureRecognized(this, e);
        }

        void sg_SwipeGestureRecognisedEventHandler(object sender, SwipeGestureRecognisedEventArgs e)
        {
            SwipeGestureRecognized(this, e);
        }
        #endregion Gesture EventHandlers
    }
}
