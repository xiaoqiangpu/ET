namespace ET
{
    /// <summary>
    /// LSCollider生命周期管理
    /// </summary>
    [EntitySystemOf(typeof(LSCollider))]
    [FriendOf(typeof(LSCollider))]
    public static partial class LSColliderSystem
    {
        
        
        [EntitySystem]
        private static void Awake(this ET.LSCollider self, ET.LSColliderType type)
        {
            self.ShapeType = type;
            
            Unit           unit          = self.GetParent<Unit>();
            UnitComponent  unitComponent = unit.GetParent<UnitComponent>();
            Scene          phyScene      = unitComponent.GetParent<Scene>();
            LSPhysicsWorld world = phyScene.GetComponent<LSPhysicsWorld>();
            world?.Colliders.Add(self);
        }

        [EntitySystem]
        private static void Destroy(this ET.LSCollider self)
        {
            Unit           unit          = self.GetParent<Unit>();
            UnitComponent  unitComponent = unit.GetParent<UnitComponent>();
            Scene          phyScene      = unitComponent.GetParent<Scene>();
            LSPhysicsWorld world         = phyScene.GetComponent<LSPhysicsWorld>();
            world?.Colliders.Remove(self);
        }
    }
}