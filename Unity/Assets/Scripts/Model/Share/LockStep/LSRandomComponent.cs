using TrueSync;

namespace ET
{
    [ComponentOf(typeof(LSWorld))]
    public class LSRandomComponent:LSEntity,IAwake<uint>,IDestroy
    {
        public TSRandom Random;
    }
}