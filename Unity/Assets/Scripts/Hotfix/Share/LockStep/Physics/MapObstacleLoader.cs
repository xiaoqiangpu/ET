using System.IO;
using MongoDB.Bson;
using TrueSync;

namespace ET
{
    [FriendOf(typeof(ET.LSCollider))]
    [FriendOf(typeof(LSPhysicsWorld))]
    public static class MapObstacleLoader
    {
        public static void Load(Scene lsScene, string mapName)
        {
            // 路径根据实际运行环境调整，编辑器模式下 ../Config
            string path = $"../Config/MapObstacles/{mapName}.json";

            if (!File.Exists(path)) return;

            string json = File.ReadAllText(path);
            MapObstacleConfig config = MongoHelper.FromJson<MapObstacleConfig>(json);

            if (config?.obstacles == null) return;

            foreach (var data in config.obstacles)
            {
                // 1. 创建障碍物 Unit
                // 使用 UnitType.Obstacle (需要在 UnitType枚举中添加，或者暂时用普通Unit)
                LSUnit obstacle = LSUnitFactory.CreateObstacle(lsScene);
                // 2. 设置位置 (从 Unity 导出的 float 转为 float3)
                obstacle.Position =new TSVector((FP)data.x, data.y, data.z);

                // 3. 添加碰撞组件 (Box)
                var collider = obstacle.AddComponent<LSCollider, LSColliderType>(LSColliderType.Box);

                // 4. 设置尺寸 (Unity Scale = Box Size)
                collider.Size = new TSVector(data.sx, data.sy, data.sz);
                collider.Offset = TSVector.zero;

                // 5. 标记为静态 (极其重要：物理引擎会跳过静态物体间的碰撞检测，节省性能)
                collider.IsStatic = true;

                // 6. 初始化包围盒
                LSPhysicsMath.UpdateAABB(collider);
            }
            Log.Info($"pxq--地图 {mapName} 物理阻挡加载完成，共 {config.obstacles.Count} 个。");
        }
    }
}