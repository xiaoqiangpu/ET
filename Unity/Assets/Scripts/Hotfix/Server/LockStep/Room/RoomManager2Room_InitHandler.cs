using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.RoomRoot)]
    public class RoomManager2Room_InitHandler : MessageHandler<Scene, RoomManager2Room_Init, Room2RoomManager_Init>
    {
        protected override async ETTask Run(Scene root, RoomManager2Room_Init request, Room2RoomManager_Init response)
        {
            string serverMapName = "Map3";
            
            Log.Info($"pxq--Server--LockStep--CreateRooom---sceneName:{serverMapName}---");
            
            Room room = root.AddComponent<Room>();
            // room.Name = "Server";  //pxq--测试
            room.Name = serverMapName;
            room.AddComponent<RoomServerComponent, List<long>>(request.PlayerIds);
            room.LSWorld = new LSWorld(SceneType.LockStepServer);
            
            await ETTask.CompletedTask;
        }
    }
}