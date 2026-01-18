namespace ET
{
    public static class LSSceneFactory
    {
        public static Scene Create(Entity parent, long id, long instanceId, string name)
        {
            // 1. 创建毛坯房
            Scene lsScene = EntitySceneFactory.CreateScene(parent, id, instanceId, SceneType.LockStep, name);

            // 2. 挂载物理世界 (必须!)
            lsScene.AddComponent<LSPhysicsWorld>();

            // 3. 加载墙壁数据
            MapObstacleLoader.Load(lsScene, name);

            return lsScene;
        }
    }
}