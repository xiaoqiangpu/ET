

using System.Collections.Generic;
using System.IO;

namespace ET.Server
{
    /// <summary>
    /// 地图消息帮助类
    /// </summary>
    public static partial class MapMessageHelper
    {
        /// <summary>
        /// 通知增加Unit
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="sendUnit"></param>
        public static void NoticeUnitAdd(Unit unit, Unit sendUnit)
        {
            M2C_CreateUnits createUnits = M2C_CreateUnits.Create();
            createUnits.Units.Add(UnitHelper.CreateUnitInfo(sendUnit));
            MapMessageHelper.SendToClient(unit, createUnits);
        }
        
        /// <summary>
        /// 通知移除Unit
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="sendUnit"></param>
        public static void NoticeUnitRemove(Unit unit, Unit sendUnit)
        {
            M2C_RemoveUnits removeUnits = M2C_RemoveUnits.Create();
            removeUnits.Units.Add(sendUnit.Id);
            MapMessageHelper.SendToClient(unit, removeUnits);
        }
        
        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="message"></param>
        public static void Broadcast(Unit unit, IMessage message)
        {
            (message as MessageObject).IsFromPool = false;
            Dictionary<long, EntityRef<AOIEntity>> dict = unit.GetBeSeePlayers();
            // 网络底层做了优化，同一个消息不会多次序列化
            MessageLocationSenderOneType oneTypeMessageLocationType = unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession);
            foreach (AOIEntity u in dict.Values)
            {
                oneTypeMessageLocationType.Send(u.Unit.Id, message);
            }
        }
        
        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="message"></param>
        public static void SendToClient(Unit unit, IMessage message)
        {
            unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Send(unit.Id, message);
        }
        
        /// <summary>
        /// 发送协议给Actor
        /// </summary>
        public static void Send(Scene root, ActorId actorId, IMessage message)
        {
            root.GetComponent<MessageSender>().Send(actorId, message);
        }
    }
}