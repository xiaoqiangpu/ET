using TrueSync;


namespace ET
{
    /// <summary>
    /// 碰撞体组件
    /// 可定义碰撞体形状
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    public class LSCollider : LSEntity, IAwake<LSColliderType>, IDestroy
    {
        public LSColliderType ShapeType;

        #region 通用参数

        /// <summary>
        /// 相对于中心点的偏移量（LocalPosition）
        /// </summary>
        public TSVector Offset;
        /// <summary>
        /// 是否触发器
        /// </summary>
        public bool IsTrigger;
        /// <summary>
        /// 是否静态、
        /// (优化用，例如墙壁)
        /// </summary>
        public bool IsStatic;

        #endregion
        
        #region 形状参数

        /// <summary>
        /// Shape/Capsule 半径
        /// </summary>
        public FP Radius;
        /// <summary>
        /// Capsule高度
        /// </summary>
        public FP Height;
        /// <summary>
        /// Box的大小
        /// </summary>
        public TSVector Size;

        #endregion
        
        #region 运行时缓存

        /// <summary>
        /// AABB 最小值
        /// </summary>
        public TSVector BoundsMin;
        /// <summary>
        /// AABB 最大值
        /// </summary>
        public TSVector BoundsMax;

        #endregion

        #region 辅助属性

        public TSVector WorldCenter
        {
            get
            {
                Unit     unit    =this.GetParent<Unit>();
                TSVector unitPos = new TSVector(unit.Position.x, unit.Position.y, unit.Position.z);
                return unitPos + this.Offset;
            }
        }

        #endregion

    }

}

