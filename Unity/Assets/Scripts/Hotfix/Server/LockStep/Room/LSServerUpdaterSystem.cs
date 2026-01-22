using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace ET.Server
{
    /// <summary>
    /// 帧同步-服务器端更新逻辑
    /// 收集全员输入、确定权威帧、广播指令
    /// </summary>
    [EntitySystemOf(typeof(LSServerUpdater))]
    [FriendOf(typeof(LSServerUpdater))]
    public static partial class LSServerUpdaterSystem
    {
        [EntitySystem]
        private static void Awake(this LSServerUpdater self)
        {

        }
        
        [EntitySystem]
        private static void Update(this LSServerUpdater self)
        {
            Room room = self.GetParent<Room>();
            //获取当前服务器逻辑时间(自服务器启动时间计算开始)
            long timeNow = TimeInfo.Instance.ServerFrameTime();
            //计算下一帧权威帧号
            int frame = room.AuthorityFrame + 1;
            //如果当前真实时间还没到下一帧该开始的时间，则返回
            if (timeNow < room.FixedTimeCounter.FrameTime(frame))
            {
                return;
            }
            //获取这一帧的最终输入
            OneFrameInputs oneFrameInputs = self.GetOneFrameMessage(frame);
            
            Log.Info($"pxq--LSServerUpdate--RoomName:{room.Name}--room.PlayerIds:{room.PlayerIds.ToJson()}--frame:{frame}--oneFrameInputs:{oneFrameInputs.ToJson()}");
            
            ++room.AuthorityFrame;  //更新服务器权威帧号
            //准备广播包
            //创建一个输入帧数据，并将这一帧最终输入数据拷贝进去
            OneFrameInputs sendInput = OneFrameInputs.Create();
            oneFrameInputs.CopyTo(sendInput);
            //将这一帧的权威输入广播给房间内的所有客户端
            RoomMessageHelper.BroadCast(room, sendInput);
            //服务器端同步更新这一帧的输入逻辑
            //服务器端也需要跑一遍逻辑，用于维护权威状态，以便后续校验客户端发来的Hash
            room.Update(oneFrameInputs);
        }
        
        /// <summary>
        /// 获取并整理指定帧的全员输入消息
        /// 如果有玩家掉包没发输入，服务器会在这里进行补帧逻辑(裁定)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        private static OneFrameInputs GetOneFrameMessage(this LSServerUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            //从缓冲区中取出这一帧已收到的输入
            //这些输入是由客户端通过网络协议发送给服务器的
            OneFrameInputs oneFrameInputs = frameBuffer.FrameInputs(frame);
            //将缓冲区指针移动到当前处理帧
            frameBuffer.MoveForward(frame);
            //检查全员输入是否到达，即输入人数是否等于匹配人数
            if (oneFrameInputs.Inputs.Count == LSConstValue.MatchCount)
            {
                return oneFrameInputs;
            }
            //[补帧逻辑(裁定)]
            //有玩家输入数据没有到达服务器，服务器为了游戏不卡顿，服务器必须强行推进
            //获取前一帧作为参考
            OneFrameInputs preFrameInputs = null;
            if (frameBuffer.CheckFrame(frame - 1))
            {
                preFrameInputs = frameBuffer.FrameInputs(frame - 1);
            }

            // 遍历房间内所有玩家的ID
            // 有人输入的消息没过来，给他使用上一帧的操作
            foreach (long playerId in room.PlayerIds)
            {
                if (oneFrameInputs.Inputs.ContainsKey(playerId))
                {
                    continue;
                }

                //如果前一帧该玩家有输入，则将前一帧的数据复制到这一帧(延续上一帧)；如果前一帧也没有输入，则给一个默认空帧
                if (preFrameInputs != null && preFrameInputs.Inputs.TryGetValue(playerId, out LSInput input))
                {
                    // 使用上一帧的输入
                    oneFrameInputs.Inputs[playerId] = input;
                }
                else
                {
                    oneFrameInputs.Inputs[playerId] = new LSInput();
                }
            }

            return oneFrameInputs;
        }
    }
}