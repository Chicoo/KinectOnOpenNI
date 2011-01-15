using System;
using xn;

namespace KinectOnOpenNI
{
    /// <summary>
    /// This class controls the swipe gestures Left, Right, Up and Down.
    /// A correct swipe depends on the length and the speed of the handmovement.
    /// These are thresholds that are set in the class.
    /// The class has a seperate detection of horizontal and vertical swipe.
    /// If a swipe is detected both swipe detections are reset.
    /// </summary>
    internal class SwipeGesture : HandGesture
    {
        #region Private Variables
        private float _treshHold = 250f;
        private float _speedTreshold = 1.0f;
        private bool _verticalSwipeDetecting = false;
        private bool _horizontalSwipeDetecting = false;
        private xn.Point3D _previousPoint;
        private float _previousTime;

        private xn.Point3D _verticalStartPoint;
        private Gestures _verticalSwipeDirection;
        private float _verticalStartTime;

        private xn.Point3D _horizontalStartPoint;
        private Gestures _horizontalSwipeDirection;
        private float _horizontalStartTime;        
        #endregion Private Variables

        #region Events
        internal event EventHandler<SwipeGestureRecognisedEventArgs> SwipeGestureRecognisedEventHandler;
        #endregion Events

        #region Constructors
        internal SwipeGesture()
        {
            
        }
        #endregion Constructors

        #region Override Methods
        protected override void CheckStatus()
        {
            //Get the last postion and time
            xn.Point3D lastPosition = _traceHistory.Values[_traceHistory.Values.Count - 1];
            float lastTime = _traceHistory.Keys[_traceHistory.Values.Count - 1];
            //Initiate horizontal swipe if needed 
            if (!_horizontalSwipeDetecting && _traceHistory.Values.Count >= 2)
            {
                _horizontalSwipeDetecting = true;
                _horizontalStartPoint = _previousPoint;
                _horizontalStartTime = _previousTime;
                //Detect if the swip is lefdt or rigth
                if (lastPosition.X > _horizontalStartPoint.X) _horizontalSwipeDirection = Gestures.SwipeLeft;
                else _horizontalSwipeDirection = Gestures.SwipeRight;
            }
            //Initiate vertical swipe if needed
            if (!_verticalSwipeDetecting && _traceHistory.Values.Count >= 2)
            {
                _verticalSwipeDetecting = true;
                _verticalStartPoint = _previousPoint;
                _verticalStartTime = _previousTime;
                //Detect if the swip is up or down
                if (lastPosition.Y > _verticalStartPoint.Y) _verticalSwipeDirection = Gestures.SwipeDown;
                else _verticalSwipeDirection = Gestures.SwipeUp;
            }
            else
            {
                //Check if the detecting swipe is still valid
                switch (_horizontalSwipeDirection)
                {
                    case Gestures.SwipeLeft:
                        if (lastPosition.X < _previousPoint.X)
                        {
                            float change = lastPosition.X - _horizontalStartPoint.X;
                            float duration = lastTime - _horizontalStartTime;
                            //Check if the swip is completed.
                            //This is true if the treshold is passed.
                            CheckSwipeCompleted(lastPosition, duration, change, Gestures.SwipeLeft);
                        }
                        else
                        {
                            //ResetHorizontalValues();
                        }
                        break;
                    case Gestures.SwipeRight:
                        if (_previousPoint.X < lastPosition.X)
                        {
                            float change = _horizontalStartPoint.X - lastPosition.X;
                            float duration = lastTime - _horizontalStartTime;
                            //Check if the swip is completed.
                            //This is true if the treshold is passed.
                            CheckSwipeCompleted(lastPosition, duration, change, Gestures.SwipeRight);
                        }
                        else
                        {
                            //ResetHorizontalValues();
                        }
                        break;
                }
                switch (_verticalSwipeDirection)
                {
                    case Gestures.SwipeUp:
                        if (_previousPoint.Y < lastPosition.Y)
                        {
                            float change = lastPosition.Y - _verticalStartPoint.Y;
                            float duration = lastTime - _verticalStartTime;
                            //Check if the swip is completed.
                            //This is true if the treshold is passed.
                            CheckSwipeCompleted(lastPosition, duration, change, Gestures.SwipeUp);
                        }
                        else
                        {
                            //ResetVerticalValues();
                        }
                        break;
                    case Gestures.SwipeDown:
                        if (_previousPoint.Y < lastPosition.Y)
                        {
                            float change = _verticalStartPoint.Y - lastPosition.Y;
                            float duration = lastTime - _verticalStartTime;
                            //Check if the swip is completed.
                            //This is true if the treshold is passed.
                            CheckSwipeCompleted(lastPosition, duration, change, Gestures.SwipeDown);
                        }
                        else
                        {
                            //ResetVerticalValues();
                        }
                        break;
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
        private bool CheckSwipeCompleted(xn.Point3D lastPosition, float duration, float change, Gestures detectingGesture)
        {
            if (change >= _treshHold)
            {
                //Check if the swipe was fast enough.
                //If it is to slow it is not a swipe
                float speed = ((change / 1000) / duration);
                if (speed > _speedTreshold)
                {
                    SwipeGestureRecognisedEventHandler(this, new SwipeGestureRecognisedEventArgs(detectingGesture, lastPosition));
                }
                //Reset settings
                ResetValues();
                return true;
            }
            else
            {
                switch (detectingGesture)
                {
                    case Gestures.SwipeRight:
                    case Gestures.SwipeLeft:
                        ResetHorizontalValues();
                        break;
                    case Gestures.SwipeDown:
                    case Gestures.SwipeUp:
                        ResetVerticalValues();
                        break;
                }
            }
            return false;
        }

        private void ResetHorizontalValues()
        {
            _horizontalSwipeDetecting = false;
            _horizontalStartPoint = new xn.Point3D();
            _horizontalStartTime = 0;
        }

        private void ResetVerticalValues()
        {
            _verticalSwipeDetecting = false;
            _verticalStartPoint = new xn.Point3D();
            _verticalStartTime = 0;
        }

        private void ResetValues()
        {
            ResetHorizontalValues();
            ResetVerticalValues();
        }
        #endregion  Override Methods
    }
}
