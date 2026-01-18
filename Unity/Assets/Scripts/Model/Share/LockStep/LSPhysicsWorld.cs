using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 物理世界组件
    /// 挂在Scene上，单例管理所有物理对象
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class LSPhysicsWorld:Entity,IAwake,IUpdate,IDestroy
    {
        /// <summary>
        /// 所有碰撞体
        /// 使用EntityRef包装引用，规避直接使用Entity报错
        /// </summary>
        [BsonIgnore] //物理世界运行时列表通常不需要序列化存储
        public HashSet<EntityRef<LSCollider>> Colliders = new();
        /// <summary>
        /// 3D重力
        /// </summary>
        public TSVector Gravity = new TSVector(0, -9.81f, 0);
    }
}