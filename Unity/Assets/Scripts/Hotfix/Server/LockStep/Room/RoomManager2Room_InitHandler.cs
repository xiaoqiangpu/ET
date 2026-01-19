using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.RoomRoot)]
    public class RoomManager2Room_InitHandler : MessageHandler<Scene, RoomManager2Room_Init, Room2RoomManager_Init>
    {
        protected override async ETTask Run(Scene root, RoomManager2Room_Init request, Room2RoomManager_Init response)
        {
            Log.Info(
                $"pxq--Server--Init Room------scene name:{root.Name}--InstanceId:{root.InstanceId}--request:{request.ToJson()}--response:{response.ToJson()}");
            Room room = root.AddComponent<Room>();
            // room.Name = "Server";  //pxq--测试
            room.Name = "Map3";
            room.AddComponent<RoomServerComponent, List<long>>(request.PlayerIds);
            room.LSWorld = new LSWorld(SceneType.LockStepServer);

            
            //pxq---AddPhysics-----
            
            string phySceneName = "PhysicsScene";
            Scene phyScene = EntitySceneFactory.CreateScene(room, IdGenerater.Instance.GenerateId(),
                                                            IdGenerater.Instance.GenerateInstanceId(),
                                                            SceneType.LockStep,phySceneName);
            phyScene.AddComponent<LSPhysicsWorld>();
            phyScene.AddComponent<UnitComponent>();

            // 3. 加载墙壁数据
            MapObstacleLoader.Load(phyScene,room.Name);

            //--------

            await ETTask.CompletedTask;
        }
    }
}