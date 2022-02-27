namespace SEAN.Util
{
    public class Time
    {
        public static double Milliseconds(double startMillis = 0, double time = 0)
        {
            if (time == 0)
            {
                time = UnityEngine.Time.time;
            }
            // Time.time is in seconds -> *1000 -> milliseconds
            double msecs = startMillis + 1000 * time;
            //uint sec = (uint)(msecs / 1000);
            //uint nsecs = (uint)((msecs / 1000 - sec) * 1e+9);
            //print("msecs: " + sec + "." + nsecs);
            return msecs;
        }

        public static RosMessageTypes.Std.MTime MTime(double milliseconds)
        {
            uint sec = (uint)(milliseconds / 1000);
            return new RosMessageTypes.Std.MTime
            {
                secs = sec,
                nsecs = (uint)((milliseconds / 1000 - sec) * 1e+9)
            };
        }

    }
}
