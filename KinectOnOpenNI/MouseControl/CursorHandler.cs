using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;

namespace KinectOnOpenNI
{
    internal class CursorHandler
    {
        #region Constants
        private const UInt32 MouseEventfLeftDown = 0x0002;
        private const UInt32 MouseEventfLeftUp = 0x0004;

        //Screen coord conversion factors. 
        private const double ScreenXConv = 64.0; //65535 / 1024 
        private const double ScreenYConv = 85.3; //65535 / 768 
        #endregion

        #region DllImports
        [DllImport("user32.dll")]
        private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, long dx, long dy, uint dwData, IntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);
        #endregion

        /// <summary> 
        /// Sends the mouse left click. 
        /// </summary> 
        /// <param name="location">The location.</param> 
        public static void SendMouseLeftClick(Point location)
        {
            MoveMouseTo((int)location.X, (int)location.Y);
            mouse_event(MouseEventfLeftDown, 0, 0, 0, new IntPtr());
            mouse_event(MouseEventfLeftUp, 0, 0, 0, new IntPtr());
        }

        /// <summary> 
        /// Moves the mouse to. 
        /// </summary> 
        /// <param name="x">The x.</param> 
        /// <param name="y">The y.</param> 
        public static void MoveMouseTo(int x, int y)
        {
            x = (int)(x * ScreenXConv);
            y = (int)(y * ScreenYConv);
            // Debug.WriteLine(string.Format("mouse to: {0},{1}", x, y));
            SetCursorPos(x, y);
        }
    }
}
