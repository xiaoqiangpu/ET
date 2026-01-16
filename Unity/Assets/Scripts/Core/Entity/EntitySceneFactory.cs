namespace ET
{
    public static class EntitySceneFactory
    {
        public static Scene CreateScene(Entity parent, long id, long instanceId, SceneType sceneType, string name)
        {
            Log.Info($"pxq--EntitySceneFactory.CreateScene scene Name:{name}");
            Scene scene = new(parent.Fiber(), id, instanceId, sceneType, name);
            parent?.AddChild(scene);
            return scene;
        }
    }
}