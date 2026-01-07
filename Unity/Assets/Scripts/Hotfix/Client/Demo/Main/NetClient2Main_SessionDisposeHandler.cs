namespace ET.Client
{
    /// <summary>
    /// NetClient Fiber 向 Main Fiber 派发Session
    /// </summary>
    [MessageHandler(SceneType.All)]
    public class NetClient2Main_SessionDisposeHandler: MessageHandler<Scene, NetClient2Main_SessionDispose>
    {
        protected override async ETTask Run(Scene entity, NetClient2Main_SessionDispose message)
        {
            Log.Error($"session dispose, error: {message.Error}");
            await ETTask.CompletedTask;
        }
    }
}