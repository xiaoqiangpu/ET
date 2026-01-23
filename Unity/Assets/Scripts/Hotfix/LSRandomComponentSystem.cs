using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSRandomComponent))]
    [FriendOf(typeof(LSRandomComponent))]
    public static partial class LSRandomComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.LSRandomComponent self, uint speed)
        {
            self.Random=System.Activator.CreateInstance(typeof(TSRandom),true) as TSRandom;
            // self.Random = new TSRandom();
            self.Random.Initialize((int)speed);
        }
        [EntitySystem]
        private static void Destroy(this ET.LSRandomComponent self)
        {
            self.Random = null;
        }
    }
}