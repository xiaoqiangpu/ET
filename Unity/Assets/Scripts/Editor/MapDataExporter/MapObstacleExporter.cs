using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ETEditor
{
    public class MapObstacleExporter
    {
        //障碍物数据 路径存放在 Configs/MapObstacles/ 下，方便服务器读取
       const string outputDir = "../Config/MapObstacles/";
        
        // 导出数据的结构 (临时用)
        [System.Serializable]
        public class ObstacleData
        {
            public float x, y, z;      // 中心坐标
            public float sx, sy, sz;   // 尺寸 (Size)
        }

        [System.Serializable]
        public class MapData
        {
            public List<ObstacleData> obstacles = new List<ObstacleData>();
        }

        [MenuItem("ET/Map/Export Map Obstacles (JSON)", false, 200)]
        public static void ExportMapObstacles()
        {
            // 1. 找到 Obstacles 父节点
            GameObject obstacleRoot = GameObject.Find("World/Obstacles");
            if (obstacleRoot == null)
            {
                Debug.LogError("导出失败：场景中找不到名为 'Obstacles' 的根节点！");
                return;
            }

            MapData mapData = new MapData();
            
            // 2. 遍历所有子节点的 BoxCollider
            BoxCollider[] colliders = obstacleRoot.GetComponentsInChildren<BoxCollider>();
            
            foreach (var col in colliders)
            {
                // 计算世界坐标中心
                // 注意：这里我们简单处理，假设 BoxCollider 的 center 是 (0,0,0) 且物体无旋转
                // 如果有旋转，这里导出的将是 AABB
                Bounds bounds = col.bounds;

                ObstacleData data = new ObstacleData
                {
                    // 使用 bounds.center 和 bounds.size 能够自动处理父节点的缩放
                    x = bounds.center.x,
                    y = bounds.center.y,
                    z = bounds.center.z,
                    
                    sx = bounds.size.x,
                    sy = bounds.size.y,
                    sz = bounds.size.z
                };
                
                mapData.obstacles.Add(data);
            }

            // 3. 序列化为 JSON
            string json = JsonUtility.ToJson(mapData, true);

            // 4. 保存文件
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string sceneName = SceneManager.GetActiveScene().name;
            string filePath = Path.Combine(outputDir, $"{sceneName}.json");

            File.WriteAllText(filePath, json);
            
            Debug.Log($"导出障碍物成功: {filePath}, 共 {mapData.obstacles.Count} 个阻挡。");
            AssetDatabase.Refresh();
        }
    }
}