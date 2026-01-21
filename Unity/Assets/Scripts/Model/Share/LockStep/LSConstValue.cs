namespace ET
{
    //帧同步 常量值设置
    public static class LSConstValue
    {
        public const int MatchCount = 2;
        public const int UpdateInterval = 50;   //更新频率
        public const int FrameCountPerSecond = 1000 / UpdateInterval;
        public const int SaveLSWorldFrameCount = 60 * FrameCountPerSecond;
    }
}