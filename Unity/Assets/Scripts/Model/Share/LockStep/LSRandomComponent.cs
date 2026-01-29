using MemoryPack;
using TrueSync;

namespace ET
{
    [ComponentOf(typeof(LSWorld))]
    [MemoryPackable]
    public partial class LSRandomComponent:LSEntity,IAwake<uint>,IDestroy,ISerializeToEntity
    {
        [MemoryPackOrder(0)]
        public TSRandom Random;
    }
}