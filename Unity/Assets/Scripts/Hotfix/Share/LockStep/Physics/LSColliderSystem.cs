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
            
            LSPhysicsWorld lsPhyWorld = GetPhysicsWorld(self);
            lsPhyWorld?.Colliders.Add(self);
        }

        [EntitySystem]
        private static void Destroy(this ET.LSCollider self)
        {
            LSPhysicsWorld lsPhyWorld = GetPhysicsWorld(self);
            lsPhyWorld?.Colliders.Remove(self);
        }
        
        private static LSPhysicsWorld GetPhysicsWorld(LSCollider collider)
        {
            LSUnit unit = collider.GetParent<LSUnit>();
            LSUnitComponent unitComponent = unit.GetParent<LSUnitComponent>();
            LSWorld lsWorld = unitComponent.GetParent<LSWorld>();
            return lsWorld.GetComponent<LSPhysicsWorld>();
        }
    }
}