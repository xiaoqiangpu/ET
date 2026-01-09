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
            await LSSceneChangeHelper.SceneChangeTo(root, "Map3", message.ActorId.InstanceId);
        }
    }
}