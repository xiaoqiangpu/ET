namespace ET.Client
{
    /// <summary>
    /// 长连接消息-无需响应Response
    /// </summary>
    [MessageHandler(SceneType.NetClient)]
    public class A2NetClient_MessageHandler: MessageHandler<Scene, A2NetClient_Message>
    {
        protected override async ETTask Run(Scene root, A2NetClient_Message message)
        {
            root.GetComponent<SessionComponent>().Session.Send(message.MessageObject);
            await ETTask.CompletedTask;
        }
    }
}