using System;

namespace ET.Client
{
    /// <summary>
    /// 帧同步-单帧输出逻辑处理
    /// </summary>
    [MessageHandler(SceneType.LockStep)]
    public class OneFrameInputsHandler: MessageHandler<Scene, OneFrameInputs>
    {
        protected override async ETTask Run(Scene root, OneFrameInputs input)
        {
            using var _ = input ; // 方法结束时回收消息
            Room room = root.GetComponent<Room>();
            
            Log.Info($"pxq--OneFrameInputsHandler.Run-1-权威帧: {room.AuthorityFrame}-预测帧：{room.PredictionFrame}-权威Input:{input.ToJson()}--");
                        
            FrameBuffer frameBuffer = room.FrameBuffer;
            
            ++room.AuthorityFrame;
            // 服务端返回的消息比预测的还早
            if (room.AuthorityFrame > room.PredictionFrame)
            {
                Log.Info($"pxq--OneFrameInputsHandler.Run-2-权威帧:{room.AuthorityFrame}-大于-预测帧：{room.PredictionFrame}---直接使用权威帧--需要追帧/补帧--");
                OneFrameInputs authorityFrame = frameBuffer.FrameInputs(room.AuthorityFrame);
                input.CopyTo(authorityFrame);
            }
            else
            {
                Log.Info($"pxq--OneFrameInputsHandler.Run-3-预测帧:{room.PredictionFrame}-大于-权威帧:{room.AuthorityFrame}--准备校验---");
                // 服务端返回来的消息，跟预测消息对比
                OneFrameInputs predictionInput = frameBuffer.FrameInputs(room.AuthorityFrame);
                if (!input.Equals(predictionInput))
                {
                    Log.Info($"pxq--OneFrameInputsHandler.Run-4-校验结果：预测帧:{room.PredictionFrame}-不等于-权威帧:{room.AuthorityFrame}-触发回滚>>>");
                    input.CopyTo(predictionInput);
                    // 回滚到frameBuffer.AuthorityFrame
                    Log.Info($"pxq--OneFrameInputsHandler.Run--Rollback-start-frame:{room.AuthorityFrame}");
                    LSClientHelper.Rollback(room, room.AuthorityFrame);
                    Log.Info($"pxq--OneFrameInputsHandler.Run--Rollback-finish-frame:{room.AuthorityFrame}");
                }
                else // 对比成功
                {
                    Log.Info($"pxq--OneFrameInputsHandler.Run-5-校验结果：预测帧:{room.PredictionFrame}-不等于-权威帧:{room.AuthorityFrame}-预测成功--存档>>>");
                    room.Record(room.AuthorityFrame);
                    room.SendHash(room.AuthorityFrame);
                }
            }
            await ETTask.CompletedTask;
        }
    }
}