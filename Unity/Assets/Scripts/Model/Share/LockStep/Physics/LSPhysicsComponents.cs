using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 碰撞类型
    /// </summary>
    public enum LSColliderType
    {
        Sphere,
        Box,
        Capsule,
    }

    /// <summary>
    /// 射线检测结果
    /// </summary>
    public struct LSRaycastHit
    {
        public Entity Entity;   //撞到的实体
        public TSVector Point;  //碰撞点
        public TSVector Normal; //碰撞面发现
        public FP Distance;     //距离
    }
    
    /// <summary>
    /// 物理世界组件
    /// 挂在Scene上，单例管理所有物理对象
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class LSPhysicsWorld:Entity,IAwake,IUpdate,IDestroy
    {
        /// <summary>
        /// 所有碰撞体
        /// <存储碰撞体的InstanceId,而不是对象引用>
        /// </summary>
        [BsonIgnore] //物理世界运行时列表通常不需要序列化存储
        public HashSet<long> ColliderIds = new();
        /// <summary>
        /// 3D重力
        /// </summary>
        public TSVector Gravity = new TSVector(0, -9.81f, 0);
    }

    /// <summary>
    /// 刚体组件
    /// 挂在Unit上，赋予物体物理特性（质量、速度）
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class LSRigidbody : Entity, IAwake, IDestroy
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
        public FP Drag = 0.05f;
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

