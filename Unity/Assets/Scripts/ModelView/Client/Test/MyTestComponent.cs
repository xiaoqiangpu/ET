namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class MyTestComponent : Entity, IAwake<string>, IUpdate, ILateUpdate, IDestroy
    {
        public string Name;
    }
}