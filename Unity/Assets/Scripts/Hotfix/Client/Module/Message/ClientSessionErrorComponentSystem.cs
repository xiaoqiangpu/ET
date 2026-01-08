namespace ET.Client
{
    /// <summary>
    /// 客户端NetClient 向 Main 发布Error Message
    /// </summary>
    [EntitySystemOf(typeof(ClientSessionErrorComponent))]
    public static partial class ClientSessionErrorComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ClientSessionErrorComponent self)
        {

        }
        
        [EntitySystem]
        private static void Destroy(this ClientSessionErrorComponent self)
        {
            Fiber fiber = self.Fiber();
            if (fiber.IsDisposed)
            {
                return;
            }
            NetClient2Main_SessionDispose message = NetClient2Main_SessionDispose.Create();
            message.Error = self.GetParent<Session>().Error;
            fiber.Root.GetComponent<ProcessInnerSender>().Send(new ActorId(fiber.Process, ConstFiberId.Main), message);
        }
    }
}