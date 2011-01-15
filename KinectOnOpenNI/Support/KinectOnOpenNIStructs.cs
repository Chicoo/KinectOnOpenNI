using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectOnOpenNI
{
    public struct Point3D
    {
        private float _x;
        private float _y;
        private float _z;

        public float X {get {return _x;} }
        public float Y { get { return _y; } }
        public float Z { get { return _z; } }

        public Point3D(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }
    }
}
