namespace ET.Server
{
    public static class GateMapFactory
    {
        public static async ETTask<Scene> Create(Entity parent, long id, long instanceId, string name)
        {
            await ETTask.CompletedTask;
            Log.Info($"pxq--Server--GateMapFactory--CreateScene name:{name}---");
            Scene scene = EntitySceneFactory.CreateScene(parent, id, instanceId, SceneType.Map, name);

            scene.AddComponent<UnitComponent>();
            scene.AddComponent<AOIManagerComponent>();
            scene.AddComponent<RoomManagerComponent>();
            scene.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            
            // 1. 挂载物理世界
            scene.AddComponent<LSPhysicsWorld>();
        
            // 2. 加载障碍物数据 (Share层的代码)
            MapObstacleLoader.Load(scene, name);
        
            Log.Info("pxq--服务器端 Map3 物理系统加载完毕");
            
            return scene;
        }
        
    }
}