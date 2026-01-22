using System;
using System.IO;
using MongoDB.Bson;

namespace ET.Client
{
    /// <summary>
    /// 帧同步-客户端更新逻辑
    /// 预测、收集输入、同步服务器
    /// </summary>
    [EntitySystemOf(typeof(LSClientUpdater))]
    [FriendOf(typeof (LSClientUpdater))]
    public static partial class LSClientUpdaterSystem
    {
        [EntitySystem]
        private static void Awake(this LSClientUpdater self)
        {
            Room room = self.GetParent<Room>();
            self.MyId = room.Root().GetComponent<PlayerComponent>().MyId;
        }
        /// <summary>
        /// 包含客户带预测等逻辑
        /// 最多预测5帧
        /// </summary>
        /// <param name="self"></param>
        [EntitySystem]
        private static void Update(this LSClientUpdater self)
        {
            Room room = self.GetParent<Room>();
            //当前服务器时间
            long timeNow = TimeInfo.Instance.ServerNow();
            Scene root = room.Root();
            int i = 0;
            while (true)
            {
                //如果当前真实时间还没到下一帧的时间点，则返回
                if (timeNow < room.FixedTimeCounter.FrameTime(room.PredictionFrame + 1))
                {
                    return;
                }
                // 最多只预测5帧，以免导致预测不可靠或回滚表现太夸张
                if (room.PredictionFrame - room.AuthorityFrame > 5)
                {
                    return;
                }
                ++room.PredictionFrame; //预测帧号增加
                //获取这一帧的输入(包含预测逻辑)
                OneFrameInputs oneFrameInputs = self.GetOneFrameMessages(room.PredictionFrame);
                
                Log.Info($"pxq--LSClientUpdate--RoomName:{room.Name}--room.PlayerIds:{room.PlayerIds.ToJson()}--PredictionFrame:{room.PredictionFrame}--oneFrameInputs:{oneFrameInputs.ToJson()}");
                //更新这一帧的输入操作，逻辑驱动整个LSWorld的ECS系统(例如移动等)
                room.Update(oneFrameInputs);
                //将当前帧的状态快照Hash发送给服务器，用于校验是否掉帧/不同步
                room.SendHash(room.PredictionFrame);
                room.SpeedMultiply = ++i;

                FrameMessage frameMessage = FrameMessage.Create();
                frameMessage.Frame = room.PredictionFrame;
                frameMessage.Input = self.Input;
                //将本地玩家的帧操作(玩家输入)发送给服务器
                root.GetComponent<ClientSenderComponent>().Send(frameMessage);
                
                long timeNow2 = TimeInfo.Instance.ServerNow();
                if (timeNow2 - timeNow > 5)
                {
                    break;
                }
            }
        }
        
        /// <summary>
        /// 获取单帧的消息(包含客户端预测逻辑)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        private static OneFrameInputs GetOneFrameMessages(this LSClientUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            
            //1.获取历史权威帧
            //如果获取的是已确认帧(frame <= room.AuthorityFrame)，说明这一帧的输入是经过服务器校验后下发的准确数据
            //确定性回放的基础：如果frame <= room.AuthorityFrame,代码直接返回缓冲区内容，这保证了当服务器包到来触发回滚时，
            //逻辑层能拿到正确的历史帧输入帧重新计算
            if (frame <= room.AuthorityFrame)
            {
                //则直接从frameBuffer中获取真实输入
                return frameBuffer.FrameInputs(frame);
            }
            
            //2.获取客户端预测帧
            //2.1 从缓冲区获取预测帧的输入对象
            OneFrameInputs predictionFrame = frameBuffer.FrameInputs(frame);
            //2.2 缓冲区指针向前推进，会清理掉过旧的帧数据确保能容纳当前帧
            frameBuffer.MoveForward(frame);
            //2.3 实现“输入延迟”预测策略
            //检查最后一次服务器权威帧是否存在
            if (frameBuffer.CheckFrame(room.AuthorityFrame))
            {
                //获取最近的一次权威帧输入
                OneFrameInputs authorityFrame = frameBuffer.FrameInputs(room.AuthorityFrame);
                //核心预测逻辑-[输入延续]：将最后一次权威帧的全员操作完全拷贝到当前的预测帧中
                //在网络同步还没到时，我们假设其他玩家会会维持他们上一帧的操作(例如一直按住W走等)
                authorityFrame.CopyTo(predictionFrame);
            }
            //2.4 覆盖本地玩家输入(消除本地操作延迟的关键点)
            //虽然其他玩家的操作是靠“猜”(延续上一帧)，但本地玩家的操作是实时的
            //self.Input存储了当前客户端采集到的本地玩家输入(例如按键操作等)
            //将预测中关于本地玩家的部分替换为当前的实时操作
            predictionFrame.Inputs[self.MyId] = self.Input;
            //返回这个“半真半假”的输入(其他人是假的，本地玩家我是真的)，交给Room.Update 驱动逻辑层
            return predictionFrame;
        }
    }
}