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
            LSPhysicsWorld world=self.Root().GetComponent<LSPhysicsWorld>();
            world?.ColliderIds.Add(self.InstanceId);
        }
        [EntitySystem]
        private static void Destroy(this ET.LSCollider self)
        {
            LSPhysicsWorld world=self.Root()?.GetComponent<LSPhysicsWorld>();
            world?.ColliderIds.Remove(self.InstanceId);
        }        
    }
}