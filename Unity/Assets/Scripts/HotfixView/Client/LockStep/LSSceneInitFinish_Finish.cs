namespace ET.Client
{
    /// <summary>
    /// 帧同步-场景初始化完成
    /// </summary>
    [Event(SceneType.LockStep)]
    public class LSSceneInitFinish_Finish: AEvent<Scene, LSSceneInitFinish>
    {
        protected override async ETTask Run(Scene clientScene, LSSceneInitFinish args)
        {
            Room room = clientScene.GetComponent<Room>();
            
            await room.AddComponent<LSUnitViewComponent>().InitAsync();
            
            room.AddComponent<LSCameraComponent>();

            if (!room.IsReplay)
            {
                room.AddComponent<LSOperaComponent>();
            }
            Log.Info($"pxq--帧同步场景初始化完成--添加帧同步相机等....移除大厅UI");
            await UIHelper.Remove(clientScene, UIType.UILSLobby);
            await ETTask.CompletedTask;
        }
    }
}