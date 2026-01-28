using System;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson;
using TrueSync;

namespace ET
{
    [FriendOf(typeof(Room))]
    [FriendOf(typeof(LSRandomComponent))]
    public static partial class RoomSystem
    {
        public static Room Room(this Entity entity)
        {
            return entity.IScene as Room;
        }

        public static void Init(this Room self, List<LockStepUnitInfo> unitInfos, long startTime, int frame = -1)
        {
            Log.Info($"pxq--Room.Init--name:{self.Name}--unitInfos：{unitInfos.ToJson()}---startTime：{startTime}");

            self.StartTime = startTime;
            self.AuthorityFrame = frame;
            self.PredictionFrame = frame;
            self.Replay.UnitInfos = unitInfos;
            self.FrameBuffer = new FrameBuffer(frame);
            self.FixedTimeCounter = new FixedTimeCounter(self.StartTime, 0, LSConstValue.UpdateInterval);
            LSWorld lsWorld = self.LSWorld;
            lsWorld.Frame = frame + 1;
            //---pxq---AddPhysics-----
            lsWorld.AddComponent<LSUnitComponent>();
            lsWorld.AddComponent<LSPhysicsWorld>();
            MapObstacleLoader.Load(lsWorld, self.Name);

            uint speed = (uint)self.StartTime;
            if (speed == 0) speed = 1;
            LSRandomComponent lsRandom = lsWorld.AddComponent<LSRandomComponent, uint>(speed);
            lsWorld.Random = lsRandom.Random;
            //-----------------------
            //Init UnitInfo
            for (int i = 0; i < unitInfos.Count; ++i)
            {
                LockStepUnitInfo unitInfo = unitInfos[i];
                unitInfo.Position = new TSVector(lsWorld.Random.Range(-20, 20), 0, lsWorld.Random.Range(-10, 10));
                LSUnitFactory.Init(lsWorld, unitInfo);
                self.PlayerIds.Add(unitInfo.PlayerId);
            }
        }

        public static void Update(this Room self, OneFrameInputs oneFrameInputs)
        {
            Log.Info($"pxq--RoomSystem--Update--oneFrameInputs：{oneFrameInputs.ToJson()}");
            LSWorld lsWorld = self.LSWorld;
            // 设置输入到每个LSUnit身上
            LSUnitComponent unitComponent = lsWorld.GetComponent<LSUnitComponent>();
            foreach (var kv in oneFrameInputs.Inputs)
            {
                LSUnit lsUnit = unitComponent.GetChild<LSUnit>(kv.Key);
                LSInputComponent lsInputComponent = lsUnit.GetComponent<LSInputComponent>();
                lsInputComponent.LSInput = kv.Value;
                Log.Info($"pxq--Update OneFrameInput to Player---playerId:{kv.Key}---Input:{ kv.Value.ToJson()}");
            }

            if (!self.IsReplay)
            {
                // 保存当前帧场景数据
                self.SaveLSWorld();
                self.Record(self.LSWorld.Frame);
            }

            lsWorld.Update();
        }

        public static LSWorld GetLSWorld(this Room self, SceneType sceneType, int frame)
        {
            Log.Info($"pxq--RoomSystem--GetLSWorld--获取指定帧快照--重建LSWorld--");
            MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            LSWorld lsWorld = MemoryPackHelper.Deserialize(typeof(LSWorld), memoryBuffer) as LSWorld;
            lsWorld.SceneType = sceneType;
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            return lsWorld;
        }

        private static void SaveLSWorld(this Room self)
        {
            Log.Info($"pxq--RoomSystem--SaveLSWorld--self.Root().Name:{self.Root().Name}--");
            int frame = self.LSWorld.Frame;
            MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);

            MemoryPackHelper.Serialize(self.LSWorld, memoryBuffer);
            memoryBuffer.Seek(0, SeekOrigin.Begin);

            long hash = memoryBuffer.GetBuffer().Hash(0, (int)memoryBuffer.Length);

            self.FrameBuffer.SetHash(frame, hash);
        }

        /// <summary>
        /// 记录需要存档的数据
        /// </summary>
        /// <param name="self"></param>
        /// <param name="frame"></param>
        public static void Record(this Room self, int frame)
        {
            Log.Info($"pxq--RoomSystem--Record--self.Root().Name:{self.Root().Name}--frame:{frame}--");
            if (frame > self.AuthorityFrame)
            {
                return;
            }

            OneFrameInputs oneFrameInputs = self.FrameBuffer.FrameInputs(frame);
            OneFrameInputs saveInput = OneFrameInputs.Create();
            oneFrameInputs.CopyTo(saveInput);
            self.Replay.FrameInputs.Add(saveInput);
            if (frame % LSConstValue.SaveLSWorldFrameCount == 0)
            {
                MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
                byte[] bytes = memoryBuffer.ToArray();
                self.Replay.Snapshots.Add(bytes);
            }
        }
    }
}