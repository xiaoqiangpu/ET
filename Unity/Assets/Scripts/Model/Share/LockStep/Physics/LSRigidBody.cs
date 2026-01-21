using TrueSync;
namespace ET
{
    /// <summary>
    /// 刚体组件
    /// 挂在Unit上，赋予物体物理特性（质量、速度）
    /// </summary>
    [ComponentOf(typeof(LSUnit))]
    public class LSRigidBody : LSEntity, IAwake, IDestroy
    {
        /// <summary>
        /// 线性速度
        /// </summary>
        public TSVector Velocity;
        /// <summary>
        /// 质量
        /// 0或负数视为无限大/静态
        /// </summary>
        public FP Mass = 1;
        /// <summary>
        /// 空气阻力
        /// </summary>
        public FP Drag = 0;
        /// <summary>
        /// 是否启用重力
        /// </summary>
        public bool UseGravity = true;
        /// <summary>
        /// 是否是运动学
        /// 不受碰撞回避，但可以推别人
        /// </summary>
        public bool IsKinematic;
    }
}
