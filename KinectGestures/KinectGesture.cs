using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ManagedNite;
using System.Drawing;

namespace KinectGestures
{
    #region Enums
    public enum SwipeDirection
    {
        Up,
        Down,
        Left,
        Right,
        Backward,
        Forward,
        Illigal
    }
    #endregion Enums

    #region EventArgs
    public class SessionStartedEventArgs : EventArgs
    {
        public float X;
        public float Y;
        public float Z;

        public SessionStartedEventArgs(PointEventArgs e):this(e.Point.X, e.Point.Y, e.Point.Z)
        {
        }

        public SessionStartedEventArgs(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    } 

    public class CircleDetectedEventArgs : EventArgs
    {
        public PointF CircleCenter;
        public float CircleRadius;
        public bool Confident;
        public float Value;

        public CircleDetectedEventArgs(CircleEventArgs e): this(new PointF(e.CircleCenter.X, e.CircleCenter.Y), e.CircleRadius, e.Confident, e.Value)
        {
        }

        public CircleDetectedEventArgs(PointF center, float radius, bool confident, float value)
        {
            CircleCenter = center;
            CircleRadius = radius;
            Confident = confident;
            Value = value;
        }
    }

    public class PushEventArgs : EventArgs
    {
        public uint Duration;
        public float Angle;
        public float Velocity;

        public PushEventArgs(uint duration, float angle, float velocity)
        {
            Duration = duration;
            Angle = angle;
            Velocity = velocity;
        }
    }

    public class SteadyDetectEventArgs : EventArgs
    {
        public float Velocity;

        public SteadyDetectEventArgs(float velocity)
        {
            Velocity = velocity;
        }

        public SteadyDetectEventArgs(SteadyEventArgs e) : this(e.Velocity)
        {
        }
    }

    public class SwipeEventArgs : EventArgs
    {
        public SwipeDirection Direction;
        public float Angle;
        public float Velocity;

        public SwipeEventArgs(SwipeDetectorGeneralEventArgs e)
        {
            Angle = e.Angle;
            Velocity = e.Velocity;
            if (!Enum.TryParse<SwipeDirection>(e.SelectDirection.ToString(), out Direction))
            {
                Direction = SwipeDirection.Illigal;
            }
        }

        public SwipeEventArgs(SwipeDirection direction, float angle, float velocity)
        {
            Direction = direction;
            Angle = angle;
            Velocity = velocity;
        }
    }
	#endregion EventArgs


    public class GestureHandler
    {
        #region Variables
        XnMSessionManager sessionManager;
        XnMOpenNIContext context;
        bool terminate = false;
        XnMCircleDetector circleDetector;
        XnMPushDetector pushDetector;
        XnMSwipeDetector swipeDetector;
        XnMSteadyDetector steadyDetector;
        #endregion Variables

        #region EventHandlers
        public event EventHandler<SessionStartedEventArgs> SessionStarted;
        public event EventHandler<CircleDetectedEventArgs> CircleDetected;
        public event EventHandler<PushEventArgs> PushDetected;
        public event EventHandler<SwipeEventArgs> SwipeDetected;
        public event EventHandler<SteadyDetectEventArgs> SteadyDetected;
        #endregion EventHandlers

        public GestureHandler()
        {

        }

        public void Init()
        {
            context = new XnMOpenNIContext();
            context.Init();

            sessionManager = new XnMSessionManager(context, "Click,Wave", "RaiseHand");
            sessionManager.SessionStarted += new EventHandler<PointEventArgs>(sessionManager_SessionStarted);
            sessionManager.SessionEnded += new EventHandler(sessionManager_SessionEnded);

            //slider2D = new XnMSelectableSlider2D(xItems, yItems);

            //slider2D.Activate += new EventHandler(slider2D_Activate);
            //slider2D.Deactivate += new EventHandler(slider2D_Deactivate);

            //slider2D.ValueChanged += new EventHandler<SelectableSlider2DValueChangedEventArgs>(slider2D_ValueChanged);
            //slider2D.MessageUpdate += new EventHandler<OneMessageEventArgs>(slider2D_MessageUpdate);
            //slider2D.ItemHovered += new EventHandler<SelectableSlider2DHoverEventArgs>(slider2D_ItemHovered);
            //slider2D.ItemSelected += new EventHandler<SelectableSlider2DSelectEventArgs>(slider2D_ItemSelected);

            //pointDenoiser = new XnMPointDenoiser();
            //pointDenoiser.AddListener(slider2D);

            //flowRouter = new XnMFlowRouter();
            //flowRouter.SetActiveControl(pointDenoiser);

            circleDetector = new XnMCircleDetector();
            circleDetector.Circle += new EventHandler<CircleEventArgs>(circleDetector_Circle);
            
            pushDetector = new XnMPushDetector();
            pushDetector.Push += new EventHandler<PushDetectorEventArgs>(pushDetector_Push);

            swipeDetector = new XnMSwipeDetector(true);
            swipeDetector.GeneralSwipe += new EventHandler<SwipeDetectorGeneralEventArgs>(swipeDetector_GeneralSwipe);

            steadyDetector = new XnMSteadyDetector(5, 5000);
            steadyDetector.Activate += new EventHandler(steadyDetector_Activate);
            steadyDetector.Deactivate += new EventHandler(steadyDetector_Deactivate);
            steadyDetector.Steady += new EventHandler<SteadyEventArgs>(steadyDetector_Steady);

            


            //sessionManager.AddListener(flowRouter);
            sessionManager.AddListener(circleDetector);
            sessionManager.AddListener(pushDetector);
            sessionManager.AddListener(swipeDetector);
            sessionManager.AddListener(steadyDetector);

            Thread th = new Thread(new ThreadStart(SpinInfinite));
            th.Priority = ThreadPriority.Lowest;
            th.Start();
        }

        void sessionManager_SessionEnded(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void steadyDetector_Deactivate(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void steadyDetector_Activate(object sender, EventArgs e)
        {
           // throw new NotImplementedException();
        }

        void steadyDetector_Steady(object sender, SteadyEventArgs e)
        {
            SteadyDetected(sender, new SteadyDetectEventArgs(e));
        }

        void swipeDetector_GeneralSwipe(object sender, SwipeDetectorGeneralEventArgs e)
        {
            SwipeDetected(sender, new SwipeEventArgs(e));
        }

        void pushDetector_Push(object sender, PushDetectorEventArgs e)
        {
            PushDetected(sender, new PushEventArgs(pushDetector.PushDuration, e.Angle, e.Velocity));
        }

        void circleDetector_Circle(object sender, CircleEventArgs e)
        {
            CircleDetected(sender, new CircleDetectedEventArgs(e));
        }

        void sessionManager_SessionStarted(object sender, PointEventArgs e)
        {
            SessionStarted(sender, new SessionStartedEventArgs(e.Point.X, e.Point.Y, e.Point.Z));
        }

        public void Dispose()
        {
            terminate = true;
        }

        private void SpinInfinite()
        {
            while (!terminate)
            {
                try
                {
                    uint rc = context.Update();
                    if (rc == 0)
                        sessionManager.Update(context);
                }
                catch (Exception ex)
                {
                   System.Diagnostics.Debug.WriteLine("An error has occured in SpinInfinte: " + ex.Message); 
                }
            }
            if (!terminate)//If there is no signal break the system down
            {
                if (context != null)
                {
                    context.Close();
                    context.Dispose();
                }
                //if (slider2D != null)
                //    slider2D.Dispose();

                //if (circleDetector != null)
                //    circleDetector.Dispose();

                if (sessionManager != null)
                {
                    sessionManager.ClearQueue();
                    sessionManager.EndSession();
                    sessionManager.Dispose();
                }
            }
        }
    }

}
