namespace ET.Client
{
    [EntitySystemOf(typeof(MyTestComponent))]
    [FriendOf(typeof(MyTestComponent))]
    public static partial class MyTestComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.MyTestComponent self, string args2)
        {
            self.Name = args2;
            Log.Info($"pxq--MyTestComponentSystem Awake--Name: {self.Name}");
        }
        [EntitySystem]
        private static void Update(this ET.Client.MyTestComponent self)
        {

        }
        [EntitySystem]
        private static void LateUpdate(this ET.Client.MyTestComponent self)
        {

        }
        [EntitySystem]
        private static void Destroy(this ET.Client.MyTestComponent self)
        {

        }

    }
}