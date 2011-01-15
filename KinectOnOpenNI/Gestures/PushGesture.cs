using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectOnOpenNI
{
    internal class PushGesture : HandGesture
    {
        #region Private Variables
        private float _treshHold = 400f;
        private float _speedTreshold = 0.5f;
        private bool _pushDetecting = false;
        private xn.Point3D _previousPoint;
        private xn.Point3D _startPoint;
        private float _previousTime;
        private float _startTime;
        #endregion Private Variables

        #region Events
        internal event EventHandler<PushGestureRecognisedEventArgs> PushGestureRecognisedEventHandler;
        #endregion Events

        protected override void CheckStatus()
        {
            //Get the last postion and time
            xn.Point3D lastPosition = _traceHistory.Values[_traceHistory.Values.Count - 1];
            float lastTime = _traceHistory.Keys[_traceHistory.Values.Count - 1];
            //Initiate push detection if needed 
            if (!_pushDetecting && _traceHistory.Values.Count >= 2)
            {
                //Detect if there is push. This is the case if the hand is moving towards the kinect.
                if (lastPosition.Z < _previousPoint.Z)
                {
                    _startPoint = _previousPoint;
                    _startTime = _previousTime;
                    _pushDetecting = true;
                }
            }
            else
            {
                //Check if the detecting push is still valid
                if (lastPosition.Z < _previousPoint.Z)
                {
                    float change = _startPoint.Z - lastPosition.Z;
                    float duration = lastTime - _startTime;
                    System.Diagnostics.Debug.WriteLine("Last Z:" + lastPosition.Z + " - previous Z:" + _previousPoint.Z + " - change" + change);
                    //Check if the swip is completed.
                    //This is true if the treshold is passed.
                    CheckPushCompleted(lastPosition, duration, change, Gestures.Push);
                }
                else
                {
                    ResetValues();
                }
            }
            _previousPoint = lastPosition;
            _previousTime = lastTime;
        }

        /// <summary>
        /// Checks if a swipe is completed
        /// </summary>
        /// <param name="lastPosition">The last position of the swip</param>
        /// <param name="duration">The duration of the swipe</param>
        /// <param name="change">The change in position of the swipe</param>
        /// <param name="detectingGesture">The swipe that is being checked</param>
        /// <returns>A boolean to indicate that the swipe is completed</returns>
        private bool CheckPushCompleted(xn.Point3D lastPosition, float duration, float change, Gestures detectingGesture)
        {
            if (change >= _treshHold)
            {
                //Check if the swipe was fast enough.
                //If it is to slow it is not a swipe
                float speed = ((change / 1000) / duration);
                if (speed > _speedTreshold)
                {
                    PushGestureRecognisedEventHandler(this, new PushGestureRecognisedEventArgs(detectingGesture, lastPosition, speed));
                }
                //Reset settings
                ResetValues();
                return true;
            }
            return false;
        }

        private void ResetValues()
        {
            _pushDetecting = false;
            _startPoint = new xn.Point3D();
            _startTime = 0;
        }
    }
}
