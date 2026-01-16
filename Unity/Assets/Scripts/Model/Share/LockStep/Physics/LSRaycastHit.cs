using TrueSync;

namespace ET
{
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
}