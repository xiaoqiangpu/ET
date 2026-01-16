using TrueSync;

namespace ET
{
    /// <summary>
    /// 射线检测算法库
    /// </summary>
    [FriendOf(typeof(LSPhysicsWorld))]
    [FriendOf(typeof(LSCollider))]
    public static class LSRaycastMath
    {
        /// <summary>
        /// 射线结构体
        /// </summary>
        public struct Ray
        {
            /// <summary>起点</summary>
            public TSVector Origin;
            /// <summary>方向 (必须归一化)</summary>
            public TSVector Direction;
            /// <summary>方向的倒数 (用于 AABB Slab 算法优化)</summary>
            public TSVector InvDirection; 

            public Ray(TSVector origin, TSVector direction)
            {
                Origin = origin;
                Direction = direction;
                Direction.Normalize();
                // 预计算倒数，避免除零 (如果分量为0，设为一个极大值)
                InvDirection = new TSVector(
                    Direction.x == 0 ? FP.MaxValue : FP.One / Direction.x,
                    Direction.y == 0 ? FP.MaxValue : FP.One / Direction.y,
                    Direction.z == 0 ? FP.MaxValue : FP.One / Direction.z
                );
            }
        }

        /// <summary>
        /// 全局射线检测
        /// </summary>
        /// <param name="world">物理世界</param>
        /// <param name="origin">射线起点</param>
        /// <param name="direction">射线方向</param>
        /// <param name="maxDistance">最大检测距离</param>
        /// <param name="hitInfo">返回的碰撞信息</param>
        /// <returns>是否击中目标</returns>
        public static bool Raycast(LSPhysicsWorld world, TSVector origin, TSVector direction, FP maxDistance, out LSRaycastHit hitInfo)
        {
            hitInfo = default;
            
            // 构建射线对象
            Ray ray = new Ray(origin, direction);

            FP minDistance = maxDistance;
            bool hitAny = false;
            
            foreach (EntityRef<LSCollider> colliderRef in world.Colliders)
            {
                // 1. 还原对象 (隐式转换)
                LSCollider collider = colliderRef;

                // 2. 有效性检查
                // 如果实体已被销毁或引用无效，跳过
                if (collider == null || collider.IsDisposed)
                {
                    continue;
                }
                
                // 3. 触发器通常不参与射线阻挡 (视游戏逻辑而定，这里假设忽略触发器)
                if (collider.IsTrigger)
                {
                    continue;
                }

                // 4. Broad Phase (粗略阶段): AABB 剔除
                // 先快速检测射线是否穿过物体的包围盒，如果没有，直接跳过复杂的形状计算
                // 注意：这里不需要法线，只需要知道是否相交
                if (!IntersectRayAABB(ray, collider.BoundsMin, collider.BoundsMax, out FP aabbDist, out _))
                {
                    continue; 
                }
                // 如果 AABB 距离比当前最近距离还要远，也可以提前跳过
                if (aabbDist > minDistance)
                {
                    continue;
                }

                // 5. Narrow Phase (精确阶段): 具体形状检测
                FP dist = -1;
                TSVector normal = TSVector.zero;
                bool hit = false;

                switch (collider.ShapeType)
                {
                    case LSColliderType.Sphere:
                        hit = IntersectRaySphere(ray, collider.WorldCenter, collider.Radius, out dist);
                        break;
                    case LSColliderType.Box:
                        // 对于 Box，我们直接使用 AABB 结果
                        // 如果 Box 支持旋转，这里需要将射线转到 Box 局部坐标系再检测 AABB，目前假设无旋转
                        hit = IntersectRayAABB(ray, collider.BoundsMin, collider.BoundsMax, out dist, out normal);
                        break;
                    case LSColliderType.Capsule:
                        // 胶囊体射线检测较复杂，这里使用“AABB + 球体”的近似策略
                        // 或者简化为检测上下两个半球 + 中间圆柱
                        hit = IntersectRayCapsule(ray, collider, out dist, out normal);
                        break;
                }

                // 6. 结果更新
                // 必须在最大距离内，且比当前找到的最近点更近
                if (hit && dist >= 0 && dist < minDistance)
                {
                    minDistance = dist;
                    hitAny = true;
                    
                    hitInfo.Entity = collider.GetParent<Unit>();
                    hitInfo.Distance = dist;
                    hitInfo.Point = ray.Origin + ray.Direction * dist;
                    
                    // 计算法线
                    if (collider.ShapeType == LSColliderType.Sphere)
                    {
                        // 球体法线 = (碰撞点 - 圆心).归一化
                        hitInfo.Normal = (hitInfo.Point - collider.WorldCenter).normalized;
                    }
                    else if (collider.ShapeType == LSColliderType.Box)
                    {
                        hitInfo.Normal = normal;
                    }
                    else
                    {
                        hitInfo.Normal = normal; // Capsule 会在函数内计算
                    }
                }
            }

            return hitAny;
        }

        // ==========================================================================================
        // 具体算法实现
        // ==========================================================================================

        /// <summary>
        /// 射线 vs 球体 检测
        /// </summary>
        private static bool IntersectRaySphere(Ray ray, TSVector center, FP radius, out FP distance)
        {
            distance = -1;
            TSVector m = ray.Origin - center;
            FP b = TSVector.Dot(m, ray.Direction);
            FP c = TSVector.Dot(m, m) - radius * radius;

            // 如果 c > 0 (起点在球外) 且 b > 0 (射线背离球)，则无交点
            if (c > 0 && b > 0) return false; 

            FP discr = b * b - c;
            if (discr < 0) return false; // 判别式 < 0，无解

            // 取较小的解 (最近的交点)
            distance = -b - TSMath.Sqrt(discr); 
            
            // 如果 distance < 0，说明起点在球内，此时取 distance = 0 或取另一个解
            if (distance < 0) distance = 0;     
            return true;
        }

        /// <summary>
        /// 射线 vs AABB 检测 (Slab Method 完整版)
        /// 优化了除法运算，使用预计算的 InvDirection
        /// </summary>
        private static bool IntersectRayAABB(Ray ray, TSVector min, TSVector max, out FP distance, out TSVector normal)
        {
            distance = FP.Zero;
            normal = TSVector.zero;

            FP tmin = FP.Zero;     // 进入时间
            FP tmax = FP.MaxValue; // 离开时间

            int hitAxis = -1;      // 击中轴 (0=x, 1=y, 2=z)
            FP hitSign = FP.One;   // 击中方向 (+1 或 -1)

            // --- X 轴检测 ---
            FP t1 = (min.x - ray.Origin.x) * ray.InvDirection.x;
            FP t2 = (max.x - ray.Origin.x) * ray.InvDirection.x;

            // 确保 t1 是近平面，t2 是远平面
            if (t1 > t2) { FP temp = t1; t1 = t2; t2 = temp; }

            // 更新 tmin, tmax
            if (t1 > tmin) 
            { 
                tmin = t1; 
                hitAxis = 0; 
                // 如果射线方向为负，则击中 max 面(正向面)，法线为 +1？
                // 逻辑：如果方向是负的，说明从右边射过来，击中的是 Max 面 (法线 +1, 0, 0)
                // 如果方向是正的，击中的是 Min 面 (法线 -1, 0, 0)
                hitSign = ray.Direction.x < 0 ? FP.One : -FP.One;
            }
            if (t2 < tmax) tmax = t2;
            if (tmin > tmax) return false;

            // --- Y 轴检测 ---
            t1 = (min.y - ray.Origin.y) * ray.InvDirection.y;
            t2 = (max.y - ray.Origin.y) * ray.InvDirection.y;
            if (t1 > t2) { FP temp = t1; t1 = t2; t2 = temp; }

            if (t1 > tmin) 
            { 
                tmin = t1; 
                hitAxis = 1; 
                hitSign = ray.Direction.y < 0 ? FP.One : -FP.One;
            }
            if (t2 < tmax) tmax = t2;
            if (tmin > tmax) return false;

            // --- Z 轴检测 ---
            t1 = (min.z - ray.Origin.z) * ray.InvDirection.z;
            t2 = (max.z - ray.Origin.z) * ray.InvDirection.z;
            if (t1 > t2) { FP temp = t1; t1 = t2; t2 = temp; }

            if (t1 > tmin) 
            { 
                tmin = t1; 
                hitAxis = 2; 
                hitSign = ray.Direction.z < 0 ? FP.One : -FP.One;
            }
            if (t2 < tmax) tmax = t2;
            if (tmin > tmax) return false;

            // 结果赋值
            distance = tmin;
            
            // 构造法线
            if (hitAxis == 0) normal = new TSVector(hitSign, 0, 0);
            else if (hitAxis == 1) normal = new TSVector(0, hitSign, 0);
            else if (hitAxis == 2) normal = new TSVector(0, 0, hitSign);

            return true;
        }

        /// <summary>
        /// 射线 vs 胶囊体 检测 (简化版：仅检测 AABB)
        /// 完整数学解法消耗较大，这里通常使用 AABB + 内部圆柱近似，或者直接复用 AABB
        /// 如果需要极高精度，需要计算点到线段的距离
        /// </summary>
        private static bool IntersectRayCapsule(Ray ray, LSCollider capsule, out FP distance, out TSVector normal)
        {
            // 方案 A：高精度数学解法 (复杂，容易出错)
            // 方案 B：近似解法 (此处采用)，检测胶囊体的 AABB
            // 因为在 Broad Phase 已经检测过 AABB 了，所以这里如果进来，说明肯定穿过了包围盒
            // 这里为了“轻量级”，暂且复用 AABB 逻辑，如果需要更高精度，可视为检测中间的圆柱体
            
            // TODO: 如果需要精准的胶囊体检测，需要实现 Ray vs Cylinder + Ray vs Sphere (Top/Bottom)
            // 鉴于篇幅，这里暂时回退到 AABB 检测，保证不报错且有基本阻挡
            return IntersectRayAABB(ray, capsule.BoundsMin, capsule.BoundsMax, out distance, out normal);
        }
    }
}