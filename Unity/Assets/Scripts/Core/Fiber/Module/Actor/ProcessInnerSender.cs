using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 进程间通讯发送者
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ProcessInnerSender: Entity, IAwake, IDestroy, IUpdate
    {
        public const long TIMEOUT_TIME = 40 * 1000;
        
        public int RpcId;

        public readonly Dictionary<int, MessageSenderStruct> requestCallback = new();
        
        public readonly List<MessageInfo> list = new();
    }
}