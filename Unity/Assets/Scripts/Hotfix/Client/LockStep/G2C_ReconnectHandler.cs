namespace ET.Client
{
    /// <summary>
    /// 帧同步-断线重连
    /// </summary>
    [MessageHandler(SceneType.LockStep)]
    public class G2C_ReconnectHandler: MessageHandler<Scene, G2C_Reconnect>
    {
        protected override async ETTask Run(Scene root, G2C_Reconnect message)
        {
            await LSSceneChangeHelper.SceneChangeToReconnect(root, message);
            await ETTask.CompletedTask;
        }
    }
}