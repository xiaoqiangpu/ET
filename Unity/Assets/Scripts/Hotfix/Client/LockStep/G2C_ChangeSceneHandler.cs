namespace ET.Client
{
    /// <summary>
    /// 帧同步-通知匹配成功
    /// </summary>
    [MessageHandler(SceneType.LockStep)]
    public class Match2G_NotifyMatchSuccessHandler: MessageHandler<Scene, Match2G_NotifyMatchSuccess>
    {
        protected override async ETTask Run(Scene root, Match2G_NotifyMatchSuccess message)
        {
            Log.Info($"pxq--客户端收到服务器--匹配成功消息--{message.ToJson()}--开始进入地图：");
            await LSSceneChangeHelper.SceneChangeTo(root, "Map1", message.ActorId.InstanceId);
        }
    }
}