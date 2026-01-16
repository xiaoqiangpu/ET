

using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 障碍物数据
    /// </summary>
    [EnableClass]
    public class ObstacleData
    {
        public float x, y, z;
        public float sx, sy, sz;
    }
    
    [EnableClass]
    public class MapObstacleConfig
    {
        public List<ObstacleData> obstacles = new List<ObstacleData>();
    }    
}