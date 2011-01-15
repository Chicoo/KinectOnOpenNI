using System;
using LibUsbDotNet.Main;
using LibUsbDotNet;

namespace KinectOnOpenNI
{
    public class KinectMotor
    {
        #region Fields

        private static UsbDevice MyUsbDevice;
        private static UsbDeviceFinder MyUsbFinder;
        private bool _canOperate = false;
        private bool _canTiltUp = true;
        private bool _canTiltDown = true;
        private int _angle;
        
        #endregion

        #region Properties
        /// <summary>
        /// Gets/Sets the angle of the kinect
        /// </summary>
        internal int Angle
        {
            get
            {
                //byte[] buffer = GetJointStatus();
                //int angle = buffer[8];
                //if (angle > 180) angle = angle - 255;
                return _angle;
            }
        }

        internal MotorStatus MotorStatus
        {
            get
            {
                byte[] buffer = GetJointStatus();
                byte status = buffer[9];
                return (MotorStatus)status;
            }
        }

        /// <summary>
        /// Gets a value that indicates if the Kinect motor can be operated.
        /// </summary>
        internal bool CanOperate
        {
            get { return _canOperate; }
        }
        #endregion Properties

        #region Constructors
        public KinectMotor()
        {
            //Initiate the motor.
            //If an error is thrown the device cannot be operated.
            try
            {
                InitDevice();
                _canOperate = true;
            }
            catch
            {
                _canOperate = false;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Always returns 0x22 (34) so far
        /// </summary>
        /// <returns></returns>
        internal ushort GetInitStatus()
        {
            if (!_canOperate) return 0;

            UsbSetupPacket setup = new UsbSetupPacket(0xC0, 0x10, 0x0, 0x0, 0x1);
            int len = 0;

            byte[] buf = new byte[1];
            MyUsbDevice.ControlTransfer(ref setup, buf, (ushort)buf.Length, out len);

            return buf[0];
        }

        /// <summary>
        /// Set the LED on the Kinect
        /// </summary>
        /// <param name="status">The LED status to set.</param>
        internal void SetLED(KinectLEDStatus status)
        {
            if (!_canOperate) return;
            UsbSetupPacket setup = new UsbSetupPacket(0x40, 0x06, (ushort)status, 0x0, 0x0);
            int len = 0;
            MyUsbDevice.ControlTransfer(ref setup, IntPtr.Zero, 0, out len);
        }

        /// <summary>
        /// Tilt the Kinect up
        /// </summary>
        internal void TiltUp()
        {
            int angle = _angle + 1;
            System.Diagnostics.Debug.WriteLine("Angle=" + angle);
            SetAngle((sbyte)angle);

        }

        /// <summary>
        /// Tilt the Kinect down
        /// </summary>
        internal void TitlDown()
        {
            int angle = _angle - 1;
            SetAngle((sbyte)angle);
        }

        #endregion

        #region Private Methods
        private void SetAngle(sbyte angle)
        {
            if (!_canOperate) return;

            if (!_canTiltUp && angle > 0) return;
            if (!_canTiltDown && angle < 0) return;

            ushort mappedValue = (ushort)(byte)angle;
            int currentAngle = _angle;

            UsbSetupPacket setup = new UsbSetupPacket(0x40, 0x31, mappedValue, 0x0, 0x0);
            int len = 0;
            MyUsbDevice.ControlTransfer(ref setup, IntPtr.Zero, 0, out len);

            //Check if the angle is still OK.
            //First wait till the motor stops moving
            while (MotorStatus == MotorStatus.Moving)
            {

            }

            switch (MotorStatus)
            {
                case KinectOnOpenNI.MotorStatus.Stopped:
                    _canTiltDown = true;
                    _canTiltUp = true;
                    _angle = angle;
                    break;
                case KinectOnOpenNI.MotorStatus.ReachedLimits:
                    SetAngle((sbyte)currentAngle);
                    if (currentAngle < 0) _canTiltDown = false;
                    else _canTiltUp = false;
                    break;
            }
            System.Diagnostics.Debug.WriteLine("Angle=" + Angle + " and status=" + MotorStatus);
        }

        private byte[] GetJointStatus()
        {
            //byte[] buf = new byte[9];
            UsbSetupPacket setup = new UsbSetupPacket(0xC0, 0x32, 0x0, 0x0, 0x10);
            int len = 0;
            byte[] buf = new byte[10];
            MyUsbDevice.ControlTransfer(ref setup, buf, 10, out len);
            return buf;
        }

        private void ResetKinect()
        {
            //Reset the Kinect to the intial values.
            SetAngle(0);
        }

        private void InitDevice()
        {
            MyUsbFinder = new UsbDeviceFinder(0x045E, 0x02B0);
            MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

            // If the device is open and ready
            if (MyUsbDevice == null) throw new Exception("Device Not Found.");

            // If this is a "whole" usb device (libusb-win32, linux libusb)
            // it will have an IUsbDevice interface. If not (WinUSB) the 
            // variable will be null indicating this is an interface of a 
            // device.
            IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                // This is a "whole" USB device. Before it can be used, 
                // the desired configuration and interface must be selected.

                // Select config #1
                wholeUsbDevice.SetConfiguration(1);

                // Claim interface #0.
                wholeUsbDevice.ClaimInterface(0);
            }

            _canOperate = true;
            ResetKinect();
            
        }
        #endregion

    }
}
