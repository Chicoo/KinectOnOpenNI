using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectOnOpenNI
{
    public class GestureRecognisedEventArgs : EventArgs
    {
        protected Gestures _gesture;
        protected Point3D _point;
        public Gestures DetectedGesture { get { return _gesture; } }
        public Point3D Point { get { return _point; } }

        public GestureRecognisedEventArgs()
        {
        }

        public GestureRecognisedEventArgs(Gestures gesture, Point3D point)
        {
            _gesture = gesture;
            _point = point;
        }
    }

    public class SwipeGestureRecognisedEventArgs : GestureRecognisedEventArgs
    {
        public SwipeGestureRecognisedEventArgs(Gestures gesture, Point3D point)
        {
            _gesture = gesture;
            _point = point;
        }

        public SwipeGestureRecognisedEventArgs(Gestures gesture, xn.Point3D point)
        {
            _gesture = gesture;
            _point = new Point3D(point.X, point.Y, point.Z) ;
        }
    }

    public class StillGestureRecognisedEventArgs : GestureRecognisedEventArgs
    {
        private float _timeHold;
        public float TimeHold { get { return _timeHold; } }


        public StillGestureRecognisedEventArgs(Gestures gesture, Point3D point, float fTime)
        {
            _gesture = gesture;
            _point = point;
            _timeHold = fTime;
        }

        public StillGestureRecognisedEventArgs(Gestures gesture, xn.Point3D point, float fTime)
        {
            _gesture = gesture;
            _point = new Point3D(point.X, point.Y, point.Z);
            _timeHold = fTime;
        }
    }

    public class PushGestureRecognisedEventArgs : GestureRecognisedEventArgs
    {
        private float _speed;
        public float PushSpeed { get { return _speed; } }


        public PushGestureRecognisedEventArgs(Gestures gesture, Point3D point, float speed)
        {
            _gesture = gesture;
            _point = point;
            _speed = speed;
        }

        public PushGestureRecognisedEventArgs(Gestures gesture, xn.Point3D point, float speed)
        {
            _gesture = gesture;
            _point = new Point3D(point.X, point.Y, point.Z);
            _speed = speed;
        }
    }

    public class SessionStateChangedEventArgs : EventArgs
    {
        private SessionState _newSessionState;
        private SessionState _oldSessionState;

        public SessionState NewSessionState { get { return _newSessionState; } }
        public SessionState OldSessionState { get { return _oldSessionState; } }

        public SessionStateChangedEventArgs(SessionState newSessionState, SessionState oldSessionState)
        {
            _newSessionState = newSessionState;
            _oldSessionState = oldSessionState;
        }
    }
}
