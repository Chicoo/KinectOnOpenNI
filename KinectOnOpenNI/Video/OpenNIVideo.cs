using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using xn;

namespace KinectOnOpenNI
{
    internal class OpenNIVideo
    {

        private WriteableBitmap _rgbImageSource;
        private WriteableBitmap _depthImageSource;
        //private WriteableBitmap _sceneImageSource;

        private AsyncStateData _currentState;
        private Context _niContext;
        private ImageGenerator _imageNode;
        private ImageMetaData _imageMeta;
        internal DepthGenerator _depthNode;
        private DepthMetaData _depthMeta;
        //private SceneAnalyzer _sceneNode;
        //private SceneMetaData _sceneMeta;

        private readonly DepthHistogram _depthHist = new DepthHistogram();
        private readonly SceneMap _sceneMap = new SceneMap();

        internal OpenNIVideo(OpenNIManager manager)
        {
            _niContext = manager.ONIContext;
            manager.Deactivated += new EventHandler<EventArgs>(manager_Deactivated);
        }

        void manager_Deactivated(object sender, EventArgs e)
        {
            StopTracking();
        }

        private class AsyncStateData
        {
            public readonly AsyncOperation AsyncOperation;
            public volatile bool Canceled = false;
            public volatile bool Running = true;

            public AsyncStateData(object stateData)
            {
                AsyncOperation = AsyncOperationManager.CreateOperation(stateData);
            }
        }

        internal ImageSource RgbImageSource
        {
            get { return _rgbImageSource; }
        }

        internal ImageSource DepthImageSource
        {
            get { return _depthImageSource; }
        }

        internal ImageSource SceneImageSource
        {
            get { return null; } //_sceneImageSource; }
        }

        internal event EventHandler TrackinkgCompleted;

        internal event EventHandler UpdateViewPort;

        private void InvokeUpdateViewPort(EventArgs e)
        {
            EventHandler handler = UpdateViewPort;
            if (handler != null) handler(this, e);
        }

        private void InvokeTrackinkgCompleted(EventArgs e)
        {
            EventHandler handler = TrackinkgCompleted;
            if (handler != null) handler(this, e);
        }

        internal void StartTracking()
        {
            StopTracking();

            AsyncStateData asyncData = new AsyncStateData(new object());

            TrackDelegate trackDelegate = Track;
            trackDelegate.BeginInvoke(asyncData, trackDelegate.EndInvoke, null);

            _currentState = asyncData;
        }

        internal void StopTracking()
        {
            if (_currentState != null && _currentState.Running)
                _currentState.Canceled = true;
        }

        private delegate void TrackDelegate(AsyncStateData asyncData);

        private void Track(AsyncStateData asyncData)
        {
            asyncData.Running = true;

            InitOpenNi(asyncData);
            
            while (!asyncData.Canceled)
            {
                try
                {
                    _niContext.WaitAndUpdateAll();
                    
                    // update image metadata
                    _imageNode.GetMetaData(_imageMeta);
                    _depthNode.GetMetaData(_depthMeta);
                    //_sceneNode..GetMetaData(_sceneMeta);

                    _depthHist.Update(_depthMeta);
                   // _sceneMap.Update(_sceneMeta);

                    // continue update on UI thread
                    asyncData.AsyncOperation.SynchronizationContext.Send(
                        delegate
                        {
                            // Must be called on the synchronization thread.
                            CopyWritableBitmap(_imageMeta, _rgbImageSource);

                            // CopyWritableBitmap(_depthMeta, _depthImageSource);
                            _depthHist.Paint(_depthMeta, _depthImageSource);

                            //CopyWritableBitmap(_sceneMeta, _sceneImageSource);
                            //_sceneMap.Paint(_sceneMeta, _sceneImageSource);

                            InvokeUpdateViewPort(EventArgs.Empty);
                        }, null);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("An error has occured in Track: " + ex.Message); 
                }
            }
            asyncData.Running = false;
            asyncData.AsyncOperation.PostOperationCompleted(evt => InvokeTrackinkgCompleted(EventArgs.Empty), null);
        }

        private void InitOpenNi(AsyncStateData asyncData)
        {
            //_niContext = _niContext;

            _imageNode = (ImageGenerator)_niContext.FindExistingNode(NodeType.Image);

            _imageMeta = new ImageMetaData();
            _imageNode.GetMetaData(_imageMeta);

            // create the image bitmap source on 
            asyncData.AsyncOperation.SynchronizationContext.Send(
                md => CreateImageBitmap(_imageMeta, out _rgbImageSource), 
                null);

            // add depth node
            _depthNode = (DepthGenerator)_niContext.FindExistingNode(NodeType.Depth);

            _depthMeta = new DepthMetaData();
            _depthNode.GetMetaData(_depthMeta);

            asyncData.AsyncOperation.SynchronizationContext.Send(
                state => CreateImageBitmap(_depthMeta, out _depthImageSource, PixelFormats.Pbgra32), 
                null);

            // add scene node
            //_sceneNode = (SceneAnalyzer) _niContext.FindExistingNode(NodeType.Scene);

            //_sceneMeta = new SceneMetaData();
            ////_sceneNode.GetMetaData(_sceneMeta);

            //asyncData.AsyncOperation.SynchronizationContext.Send(
            //    state => CreateImageBitmap(_sceneMeta, out _sceneImageSource, PixelFormats.Pbgra32),
            //    null);
        }

        private static void CreateImageBitmap(MapMetaData imageMd, out WriteableBitmap writeableBitmap, System.Windows.Media.PixelFormat format)
        {
            var bmpWidth = (int)imageMd.FullXRes;
            var bmpHeight = (int)imageMd.FullYRes;

            writeableBitmap = new WriteableBitmap(bmpWidth, bmpHeight, 96.0, 96.0, format, null);
        }

        private static void CreateImageBitmap(MapMetaData imageMd, out WriteableBitmap writeableBitmap)
        {
            var format = MapPixelFormat(imageMd.PixelFormat);
            CreateImageBitmap(imageMd, out writeableBitmap, format);
        }

        private static void CopyWritableBitmap(ImageMetaData imageMd, WriteableBitmap b)
        {
            IntPtr data = imageMd.ImageMapPtr;
            
            CopyWritableBitmap(imageMd, data, b);
        }

        private static void CopyWritableBitmap(SceneMetaData imageMd, WriteableBitmap b)
        {
            IntPtr data = imageMd.SceneMapPtr;

            CopyWritableBitmap(imageMd, data, b);
        }

        private static void CopyWritableBitmap(DepthMetaData imageMd, WriteableBitmap b)
        {
            IntPtr data = imageMd.DepthMapPtr;

            CopyWritableBitmap(imageMd, data, b);
        }

        private static void CopyWritableBitmap(MapMetaData imageMd, IntPtr data, WriteableBitmap b)
        {
            int dataSize = (int) imageMd.DataSize;
            
            var rect = new Int32Rect((int) imageMd.XOffset, (int) imageMd.YOffset, 
                (int) imageMd.XRes, (int) imageMd.YRes);

            b.WritePixels(rect, data, dataSize, b.BackBufferStride);
        }

        private static void CopyBitmap(IntPtr data, uint dataSize, Bitmap b)
        {
            var pf = b.PixelFormat;
            var rect = new Rectangle(0, 0, b.Width, b.Height);

            System.Drawing.Imaging.BitmapData bmpData = b.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, pf);
            //Marshal.Copy(data, bmpData.Scan0, 0, (int)dataSize);
            NativeMethods.RtlMoveMemory(bmpData.Scan0, data, dataSize);
            b.UnlockBits(bmpData);

            // workaround for InteropBitmap memory leak
            //https://connect.microsoft.com/VisualStudio/feedback/details/603004/massive-gpu-memory-leak-with-interopbitmap
            //GC.Collect(1);
        }

        private static System.Windows.Media.PixelFormat MapPixelFormat(xn.PixelFormat xnMPixelFormat)
        {
            switch (xnMPixelFormat)
            {
                case xn.PixelFormat.Grayscale8Bit:
                    return PixelFormats.Gray8;
                case xn.PixelFormat.Grayscale16Bit:
                    return PixelFormats.Gray16;
                case xn.PixelFormat.RGB24:
                    return PixelFormats.Rgb24;

                case xn.PixelFormat.YUV422:
                default:
                    throw new NotSupportedException();
            }
        }

        private static class NativeMethods
        {
            [DllImport("kernel32", EntryPoint = "CreateFileMapping", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

            [DllImport("kernel32", EntryPoint = "CloseHandle", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr handle);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern void RtlMoveMemory(IntPtr dest, IntPtr src, uint len);

            public static readonly uint FILE_MAP_ALL_ACCESS = 0xF001F;

            public const uint PAGE_READWRITE = 0x04;

            public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        }
    }
}
