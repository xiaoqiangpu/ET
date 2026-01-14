using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 房间 Component
    /// </summary>
    [ComponentOf]
    public class Room: Entity, IScene, IAwake, IUpdate
    {
        public Fiber Fiber { get; set; }
        public SceneType SceneType { get; set; } = SceneType.Room;
        public string Name { get; set; }
        
        public long StartTime { get; set; }
        
        /// <summary>
        /// 帧缓存Buffer
        /// </summary>
        public FrameBuffer FrameBuffer { get; set; }

        /// <summary>
        /// 计算fixedTime，fixedTime在客户端是动态调整的，会做时间膨胀缩放
        /// </summary>
        public FixedTimeCounter FixedTimeCounter { get; set; }

        /// <summary>
        /// 玩家id列表
        /// </summary>
        public List<long> PlayerIds { get; } = new(LSConstValue.MatchCount);
        
        /// <summary>
        /// 预测帧
        /// </summary>
        public int PredictionFrame { get; set; } = -1;

        /// <summary>
        /// 权威帧
        /// </summary>
        public int AuthorityFrame { get; set; } = -1;

        /// <summary>
        /// 回放
        /// </summary>
        public Replay Replay { get; set; } = new();

        private EntityRef<LSWorld> lsWorld;

        /// <summary>
        /// LSWorld做成child，可以有多个lsWorld，比如守望先锋有两个
        /// </summary>
        public LSWorld LSWorld
        {
            get
            {
                return this.lsWorld;
            }
            set
            {
                this.AddChild(value);
                this.lsWorld = value;
            }
        }
        /// <summary>
        /// 是否回放中
        /// </summary>
        public bool IsReplay { get; set; }
        
        /// <summary>
        /// 速度倍数
        /// </summary>
        public int SpeedMultiply { get; set; }
    }
}