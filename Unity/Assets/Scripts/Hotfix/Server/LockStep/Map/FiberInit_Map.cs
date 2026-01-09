using System.Net;

namespace ET.Server
{
    /// <summary>
    /// 帧同步-初始化地图
    /// </summary>
    [Invoke((long)SceneType.Map)]
    public class FiberInit_Map: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            root.AddComponent<UnitComponent>();
            root.AddComponent<AOIManagerComponent>();
            root.AddComponent<RoomManagerComponent>();
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<MessageLocationSenderComponent>();

            await ETTask.CompletedTask;
        }
    }
}