namespace ET.Server
{
    public static class GateMapFactory
    {
        public static async ETTask<Scene> Create(Entity parent, long id, long instanceId, string name)
        {
            await ETTask.CompletedTask;
            Log.Info($"pxq--Server--GateMapFactory--CreateScene name:{name}---");
            Scene scene = null;
            if (name != "Map3")
            {
                scene = EntitySceneFactory.CreateScene(parent, id, instanceId, SceneType.Map, name);
            }
            else
            {
                scene = LSSceneFactory.Create(parent, id, instanceId, name);
            }
            
            scene.AddComponent<UnitComponent>();
            scene.AddComponent<AOIManagerComponent>();
            scene.AddComponent<RoomManagerComponent>();
            scene.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            
            return scene;
        }
        
    }
}