using ET;
namespace ET.Client
{
    /// <summary>
    /// 场景切换
    /// </summary>
    /// <param name="root"></param>
    /// <param name="sceneName"></param>
    /// <param name="sceneInstanceId"></param>
    public static partial class SceneChangeHelper
    {
        public static async ETTask SceneChangeTo(Scene root, string sceneName, long sceneInstanceId)
        {
            root.RemoveComponent<AIComponent>();
            
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene，创建新的
            Scene currentScene = CurrentSceneFactory.Create(sceneInstanceId, sceneName, currentScenesComponent);
            
            //----增加物理组件---pxq--
            
            // 挂载物理世界组件 (管理所有碰撞体)
            currentScene.AddComponent<LSPhysicsWorld>();
            
            // 加载地图静态障碍物 (读取 Json 生成 BoxCollider)
            // 确保 MapObstacleLoader 位于 Share 层
            MapObstacleLoader.Load(currentScene, sceneName);
            
            Log.Info($"pxq--Client---CreateScene--物理系统加载完毕");
            //-------------------
            
            UnitComponent unitComponent = currentScene.AddComponent<UnitComponent>();
            
            // 可以订阅这个事件中创建Loading界面
            EventSystem.Instance.Publish(root, new SceneChangeStart());
            // 等待CreateMyUnit的消息
            Wait_CreateMyUnit waitCreateMyUnit = await root.GetComponent<ObjectWait>().Wait<Wait_CreateMyUnit>();
            M2C_CreateMyUnit m2CCreateMyUnit = waitCreateMyUnit.Message;
            Unit unit = UnitFactory.Create(currentScene, m2CCreateMyUnit.Unit);
            unitComponent.Add(unit);
            root.RemoveComponent<AIComponent>();
            
            EventSystem.Instance.Publish(currentScene, new SceneChangeFinish());
            // 通知等待场景切换的协程
            root.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
        }
    }
}