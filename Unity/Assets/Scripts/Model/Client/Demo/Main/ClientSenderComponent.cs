namespace ET.Client
{
    /// <summary>
    /// 客户端通过此组件发送消息给NetClient
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ClientSenderComponent: Entity, IAwake, IDestroy
    {
        public int fiberId;

        public ActorId netClientActorId;
    }
}