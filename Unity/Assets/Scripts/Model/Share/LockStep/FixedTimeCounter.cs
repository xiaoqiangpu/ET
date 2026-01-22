namespace ET
{
    public class FixedTimeCounter: Object
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        private long startTime;
        /// <summary>
        /// 开始帧率
        /// </summary>
        private int startFrame;
        /// <summary>
        /// 时间间隔
        /// </summary>
        public int Interval { get; private set; }

        public FixedTimeCounter(long startTime, int startFrame, int interval)
        {
            this.startTime = startTime;
            this.startFrame = startFrame;
            this.Interval = interval;
        }
        
        public void ChangeInterval(int interval, int frame)
        {
            this.startTime += (frame - this.startFrame) * this.Interval;
            this.startFrame = frame;
            this.Interval = interval;
        }

        public long FrameTime(int frame)
        {
            return this.startTime + (frame - this.startFrame) * this.Interval;
        }
        
        public void Reset(long time, int frame)
        {
            this.startTime = time;
            this.startFrame = frame;
        }
    }
}