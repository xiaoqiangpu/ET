namespace ET.Client
{
    /// <summary>
    /// 当前场景工厂
    /// </summary>
    public static class CurrentSceneFactory
    {
        public static Scene Create(long id, string name, CurrentScenesComponent currentScenesComponent)
        {
            Log.Info($"pxq--Client--CurrentSceneFactory--CreateScene name:{name}");

            Scene currentScene =
                    EntitySceneFactory.CreateScene(currentScenesComponent, id, IdGenerater.Instance.GenerateInstanceId(), SceneType.Current, name);
            currentScenesComponent.Scene = currentScene;

            EventSystem.Instance.Publish(currentScene, new AfterCreateCurrentScene());
            return currentScene;
        }
    }
}