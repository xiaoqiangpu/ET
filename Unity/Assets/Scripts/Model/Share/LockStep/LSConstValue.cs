namespace ET
{
    //帧同步 常量值设置
    public static class LSConstValue
    {
        /// <summary>
        /// 匹配数量
        /// </summary>
        public const int MatchCount = 2;
        /// <summary>
        /// 更新间隔
        /// </summary>
        public const int UpdateInterval = 50;
        /// <summary>
        /// 每秒帧数
        /// </summary>
        public const int FrameCountPerSecond = 1000 / UpdateInterval;
        /// <summary>
        /// 保存LS世界帧数
        /// </summary>
        public const int SaveLSWorldFrameCount = 60 * FrameCountPerSecond;
    }
}