using MemoryPack;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 碰撞体组件
    /// 可定义碰撞体形状
    /// </summary>
    [MemoryPackable]
    [ComponentOf(typeof(LSUnit))]
    public partial class LSCollider : LSEntity, IAwake<LSColliderType>, IDestroy, ISerializeToEntity
    {
        [MemoryPackOrder(0)]
        public LSColliderType ShapeType;

        #region 通用参数

        /// <summary>
        /// 相对于中心点的偏移量（LocalPosition）
        /// </summary>
        [MemoryPackOrder(1)]
        public TSVector Offset;

        /// <summary>
        /// 是否触发器
        /// </summary>
        [MemoryPackOrder(2)]
        public bool IsTrigger;

        /// <summary>
        /// 是否静态、
        /// (优化用，例如墙壁)
        /// </summary>
        [MemoryPackOrder(3)]
        public bool IsStatic;

        #endregion

        #region 形状参数

        /// <summary>
        /// Shape/Capsule 半径
        /// </summary>
        [MemoryPackOrder(4)]
        public FP Radius;

        /// <summary>
        /// Capsule高度
        /// </summary>
        [MemoryPackOrder(5)]
        public FP Height;

        /// <summary>
        /// Box的大小
        /// </summary>
        [MemoryPackOrder(6)]
        public TSVector Size;

        #endregion

        #region 运行时缓存

        /// <summary>
        /// AABB 最小值
        /// </summary>
        [MemoryPackOrder(7)]
        public TSVector BoundsMin;

        /// <summary>
        /// AABB 最大值
        /// </summary>
        [MemoryPackOrder(8)]
        public TSVector BoundsMax;

        #endregion

        #region 辅助属性

        public TSVector WorldCenter
        {
            get
            {
                LSUnit   unit    = this.GetParent<LSUnit>();
                TSVector unitPos = unit.Position;
                return unitPos + this.Offset;
            }
        }

        #endregion
    }
}