using UnityEngine.SceneManagement;

namespace ET.Client
{
    /// <summary>
    /// 帧同步-场景切换
    /// </summary>
    [Event(SceneType.LockStep)]
    public class LSSceneChangeStart_AddComponent: AEvent<Scene, LSSceneChangeStart>
    {
        protected override async ETTask Run(Scene clientScene, LSSceneChangeStart args)
        {
            Room room = clientScene.GetComponent<Room>();
            ResourcesLoaderComponent resourcesLoaderComponent = room.AddComponent<ResourcesLoaderComponent>();
            room.AddComponent<UIComponent>();
            
            // 创建loading界面
            
            // 创建房间UI
            await UIHelper.Create(args.Room, UIType.UILSRoom, UILayer.Low);
            
            Log.Info($"pxq--帧同步--场景切换--current scene name:{room.Name}");
            
            // 加载场景资源
            await resourcesLoaderComponent.LoadSceneAsync($"Assets/Bundles/Scenes/{room.Name}.unity", LoadSceneMode.Single);
        }
    }
}