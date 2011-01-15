using System;
using xn;
using System.Collections.Generic;

namespace KinectOnOpenNI
{
    abstract class HandGesture
    {
        #region Variables
        protected HandsGenerator _handsGenerator;
        protected OpenNIManager _manager;
        protected uint _id;
        protected bool _isTracking;
        protected SortedList<float, xn.Point3D> _traceHistory;

        private float _startTime = 0;
        #endregion  Variables

        #region Properties
        /// <summary>
        /// Gets a boolean to indicate that the gesture is tracking a hand.
        /// </summary>
        internal bool IsTracking { get { return _isTracking; } }
        #endregion Properties

        #region Internal methods
        internal void Init(OpenNIManager manager)
        {
            _manager = manager;
            _handsGenerator = _manager.ONIContext.FindExistingNode(NodeType.Hands) as HandsGenerator;
            _handsGenerator.HandCreate += new HandsGenerator.HandCreateHandler(_handGenerator_HandCreate);
            _handsGenerator.HandDestroy += new HandsGenerator.HandDestroyHandler(_handGenerator_HandDestroy);
            _handsGenerator.HandUpdate += new HandsGenerator.HandUpdateHandler(_handGenerator_HandUpdate);
            _handsGenerator.GenerationRunningChanged += new StateChangedHandler(_handsGenerator_GenerationRunningChanged);
            _handsGenerator.StartGenerating();
        }

        internal void Dispose()
        {
            _handsGenerator.StopGenerating();
            _handsGenerator.Dispose();
        }

        internal virtual void StopTracking()
        {
            if(_handsGenerator.IsValid) _handsGenerator.StopTracking(_id);
            _isTracking = false;
        }

        internal virtual void StartTracking(xn.Point3D point)
        {
            _handsGenerator.StartTracking(ref point);
            _isTracking = true;
        }
        #endregion Internal methods

        #region Protected Methods
        abstract protected void CheckStatus();
        #endregion Protected Methods

        #region Events
        internal event EventHandler<EventArgs> HandDestroyed;
        #endregion Events

        #region Eventhandlers
        void _handGenerator_HandUpdate(ProductionNode node, uint id, ref xn.Point3D position, float fTime)
        {
            if (id == _id)
            {
                float timeTracking = fTime - _startTime;
                _traceHistory.Add(timeTracking, position);
                CheckStatus();
                System.Diagnostics.Debug.WriteLine("Hand update for " + timeTracking);
            }
        }
        void _handGenerator_HandDestroy(ProductionNode node, uint id, float fTime)
        {
            if (id == _id)
            {
                _traceHistory = null;
                _startTime = 0;
                _isTracking = false;
                HandDestroyed(this, EventArgs.Empty);
            }
            System.Diagnostics.Debug.WriteLine("Hand destroy");
        }
        void _handGenerator_HandCreate(ProductionNode node, uint id, ref xn.Point3D position, float fTime)
        {
            if (_startTime == 0)
            {
                _traceHistory = new SortedList<float, xn.Point3D>();
                _startTime = fTime;
                _traceHistory.Add(0, position);
                _id = id;
                System.Diagnostics.Debug.WriteLine("Hand created");
            }
            else
            {
                _handsGenerator.StopTracking(id);
            }


        }
        void _handsGenerator_GenerationRunningChanged(ProductionNode node)
        {
        }
        #endregion Eventhandlers
    }
}
