using System;
using xn;

namespace KinectOnOpenNI
{
    internal class StillGesture : HandGesture
    {
        #region Local Variables
        private float _treshHold = 10;
        private float _timeToHold = 5;
        private xn.Point3D _startPoint;
        private float _startTime = -1;
        #endregion Local Variables

        #region Events
        internal event EventHandler<StillGestureRecognisedEventArgs> StillGestureRecognized;
        #endregion Events

        #region Override Methods
        protected override void CheckStatus()
        {
            //If starttime = -1 then iniate values.
            xn.Point3D lastPosition = _traceHistory.Values[_traceHistory.Values.Count - 1];
            float lastTime = _traceHistory.Keys[_traceHistory.Values.Count - 1];
            if (_startTime == -1)
            {
                _startPoint = lastPosition;
                _startTime = lastTime;
                return;
            }

            float xMove = lastPosition.X - _startPoint.X;
            float yMove = lastPosition.Y - _startPoint.Y;
            float zMove = lastPosition.Z - _startPoint.Z;
            float timeRunning = lastTime - _startTime;

            bool keepTracking = false;
            keepTracking = System.Math.Abs(xMove) <= _treshHold;
            if (keepTracking) keepTracking = System.Math.Abs(yMove) <= _treshHold;
            if (keepTracking) keepTracking = System.Math.Abs(zMove) <= _treshHold;

            if (!keepTracking)
            {
                //Reset time and point
                _startPoint = lastPosition;
                _startTime = lastTime;
            }
            if (keepTracking)
            {
                //Kept still long enough so this is a still getsure.
                if (timeRunning > _timeToHold)
                {
                    StillGestureRecognized(this, new StillGestureRecognisedEventArgs(Gestures.Still, _startPoint, timeRunning));
                    //Reset time and point
                    _startPoint = lastPosition;
                    _startTime = lastTime;
                }
            }
        }
        #endregion  Override Methods
    }
}
