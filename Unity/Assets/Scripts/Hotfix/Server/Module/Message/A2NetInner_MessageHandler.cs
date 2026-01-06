using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 不同进程间网络消息通讯
    /// </summary>
    [MessageHandler(SceneType.NetInner)]
    public class A2NetInner_MessageHandler: MessageHandler<Scene, A2NetInner_Message>
    {
        protected override async ETTask Run(Scene root, A2NetInner_Message innerMessage)
        {
            root.GetComponent<ProcessOuterSender>().Send(innerMessage.ActorId, innerMessage.MessageObject);
            await ETTask.CompletedTask;
        }
    }
}