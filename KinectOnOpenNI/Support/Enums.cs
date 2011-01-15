namespace KinectOnOpenNI
{
    public enum Gestures
    {
        SwipeAll,
        SwipeUp,
        SwipeDown,
        SwipeLeft,
        SwipeRight,
        Circle,
        Still,
        Push
    }

    public enum KinectLEDStatus
    {
        Off = 0x0,
        Green = 0x1,
        Red = 0x2,
        Yellow = 0x3,
        BlinkingYellow = 0x4,
        BlinkingGreen = 0x5,
        AlternateRedYellow = 0x6,
        AlternateRedGreen = 0x7
    }

    public enum MotorStatus
    {
        Stopped = 0x0,
        ReachedLimits = 0x1,
        Moving = 0x4
    }

    public enum SessionState
    {
        Starting,
        InProgress,
        Halted,
        Stopped
    }

}
