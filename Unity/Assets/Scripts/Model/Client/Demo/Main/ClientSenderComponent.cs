namespace ET.Client
{
    //客户端通过此组件发送消息给NetClient
    [ComponentOf(typeof(Scene))]
    public class ClientSenderComponent: Entity, IAwake, IDestroy
    {
        public int fiberId;

        public ActorId netClientActorId;
    }
}