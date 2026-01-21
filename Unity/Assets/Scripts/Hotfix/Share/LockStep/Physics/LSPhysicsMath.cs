using TrueSync;

namespace ET
{
    /// <summary>
    /// 3D数学算法库
    /// </summary>
    [FriendOf(typeof(LSCollider))]
    public static class LSPhysicsMath
    {
        /// <summary>
        /// AABB(轴对齐包围盒)更新与检测
        /// </summary>
        /// <param name="c">LSCollider</param>
        public static void UpdateAABB(LSCollider c)
        {
            TSVector center = c.WorldCenter;
            if (c.ShapeType == LSColliderType.Sphere)
            {
                TSVector r = new TSVector(c.Radius, c.Radius, c.Radius);
                c.BoundsMin = center - r;
                c.BoundsMax = center + r;
            }
            else if (c.ShapeType == LSColliderType.Box)
            {
                TSVector half = c.Size * FP.Half;
                c.BoundsMin = center - half;
                c.BoundsMax = center + half;
            }
            else if (c.ShapeType == LSColliderType.Capsule)
            {
                // 简化估算：胶囊体AABB = 半径+高度的一半
                FP       totalH  = (c.Height * FP.Half) + c.Radius;
                TSVector extents = new TSVector(c.Radius, totalH, c.Radius);
                c.BoundsMin = center - extents;
                c.BoundsMax = center + extents;
            }
        }

        /// <summary>
        /// 检查AABB包围盒是否重叠
        /// </summary>
        /// <param name="a">LSCollider</param>
        /// <param name="b">LSCollider</param>
        /// <returns></returns>
        public static bool CheckAABB(LSCollider a, LSCollider b)
        {
            // 3D AABB 检测：在 X, Y, Z 三个轴上都有重叠
            return (a.BoundsMin.x <= b.BoundsMax.x && a.BoundsMax.x >= b.BoundsMin.x) &&
                    (a.BoundsMin.y <= b.BoundsMax.y && a.BoundsMax.y >= b.BoundsMin.y) &&
                    (a.BoundsMin.z <= b.BoundsMax.z && a.BoundsMax.z >= b.BoundsMin.z);
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        /// <param name="c1">LSCollider</param>
        /// <param name="c2">LSCollider</param>
        /// <param name="normal">TSVector,碰撞法线，从c2指向c1</param>
        /// <param name="depth">FP，穿透深度</param>
        /// <returns></returns>
        public static bool Intersect(LSCollider c1, LSCollider c2, out TSVector normal, out FP depth)
        {
            normal = TSVector.zero;
            depth = 0;

            // 1. Sphere(c1) vs Sphere(c2)
            if (c1.ShapeType == LSColliderType.Sphere && c2.ShapeType == LSColliderType.Sphere)
            {
                // SphereVsSphere 内部计算是 p1 - p2，方向指向 p1 (c1)。符合标准。
                return SphereVsSphere(c1.WorldCenter, c1.Radius, c2.WorldCenter, c2.Radius, out normal, out depth);
            }

            // 2. Sphere(c1) vs Box(c2)
            if (c1.ShapeType == LSColliderType.Sphere && c2.ShapeType == LSColliderType.Box)
            {
                // SphereVsBox 返回的法线是指向 Sphere (c1) 的。符合标准。
                return SphereVsBox(c1.WorldCenter, c1.Radius, c2.WorldCenter, c2.Size, out normal, out depth);
            }

            // 3. Box(c1) vs Sphere(c2)
            if (c1.ShapeType == LSColliderType.Box && c2.ShapeType == LSColliderType.Sphere)
            {
                // SphereVsBox 返回的法线是指向 Sphere (c2) 的。
                bool hit = SphereVsBox(c2.WorldCenter, c2.Radius, c1.WorldCenter, c1.Size, out normal, out depth);
                
                // 【核心修正】
                // 我们的标准是：Normal 指向 c1。
                // 现在的 Normal 指向 c2。
                // 所以必须取反！
                normal = -1*normal; 
                return hit;
            }

            return false;
        }

        #region 碰撞算法实现

        private static bool SphereVsSphere(TSVector p1, FP r1, TSVector p2, FP r2, out TSVector normal, out FP depth)
        {
            normal = TSVector.zero;
            depth = 0;
            TSVector diff      = p1 - p2;
            FP       distSq    = diff.sqrMagnitude;
            FP       radiusSum = r1 + r2;

            if (distSq >= radiusSum * radiusSum) return false;

            FP dist = TSMath.Sqrt(distSq);
            if (dist == 0) // 重合了
            {
                normal = TSVector.up;
                depth = radiusSum;
            }
            else
            {
                normal = diff / dist;
                depth = radiusSum - dist;
            }

            return true;
        }

        // 3D Sphere vs AABB Box
        private static bool SphereVsBox(TSVector sphereCenter, FP radius, TSVector boxCenter, TSVector boxSize, out TSVector normal, out FP depth)
        {
            normal = TSVector.zero;
            depth = 0;
            
            // 计算半长宽
            TSVector halfSize = boxSize * FP.Half;

            // 1. 将球心转换到盒子的局部坐标系 (假设盒子无旋转)
            TSVector localPos = sphereCenter - boxCenter;

            // 2. 在盒子上寻找距离球心最近的点 (Clamped Point)
            TSVector closest = localPos;
            closest.x = TSMath.Clamp(closest.x, -halfSize.x, halfSize.x);
            closest.y = TSMath.Clamp(closest.y, -halfSize.y, halfSize.y);
            closest.z = TSMath.Clamp(closest.z, -halfSize.z, halfSize.z);

            // 3. 计算球心到最近点的向量
            TSVector diff   = localPos - closest;
            FP       distSq = diff.sqrMagnitude;
            
            // =======================================================
            // 【情况 A】球心在盒子外面 (常规情况)
            // =======================================================
            if (distSq > 0)
            {
                // 如果距离超过半径，没撞上
                if (distSq > radius * radius) return false;

                FP dist = TSMath.Sqrt(distSq);
                normal = diff / dist; // 方向：从最近点指向球心
                depth = radius - dist;
                return true;
            }

            // =======================================================
            // 【情况 B】球心在盒子内部 (穿透/深坑情况) - 核心修复 !!!
            // =======================================================
            // 此时 localPos == closest，diff 为 0。
            // 玩家掉进地里，或者出生在地里时会发生这种情况。
            // 我们需要找到离球心最近的那个面，把他推出去。

            // 计算到各个面的距离 (绝对值)
            FP distX = halfSize.x - TSMath.Abs(localPos.x);
            FP distY = halfSize.y - TSMath.Abs(localPos.y);
            FP distZ = halfSize.z - TSMath.Abs(localPos.z);

            // 找到最小穿透深度，沿着那个轴推
            // 对于地面来说，distY 通常是最小的 (因为地面很薄或者人是从上面掉下来的)
            if (distY < distX && distY < distZ)
            {
                // 推向 Y 轴最近的一侧 (如果人在上半部分就往上推，下半部分往下推)
                normal = localPos.y > 0 ? TSVector.up : TSVector.down;
                // [优化] 不要直接加 Radius，这会导致球“跳”到地面上
                // 只要推到表面即可，深度就是 distY (距离表面的距离) + Radius (球心距离表面的距离)
                // 之前的逻辑 depth = distY + radius 其实是对的，但为了稳妥，我们加上 0.001 的微小偏移
                depth = distY + radius; // 推出表面 + 半径
            }
            else if (distX < distZ)
            {
                normal = localPos.x > 0 ? TSVector.right : TSVector.left;
                depth = distX + radius;
            }
            else
            {
                normal = localPos.z > 0 ? TSVector.forward : TSVector.back;
                depth = distZ + radius;
            }

            return true;
        }

        private static bool AABBVsAABB(LSCollider c1, LSCollider c2, out TSVector normal, out FP depth)
        {
            normal = TSVector.zero;
            depth = 0;

            // 计算两物体中心距离
            TSVector center1 = c1.WorldCenter;
            TSVector center2 = c2.WorldCenter;
            TSVector diff    = center1 - center2;

            // 计算重叠量
            FP overlapX = (c1.Size.x + c2.Size.x) * FP.Half - TSMath.Abs(diff.x);
            if (overlapX <= 0) return false;

            FP overlapY = (c1.Size.y + c2.Size.y) * FP.Half - TSMath.Abs(diff.y);
            if (overlapY <= 0) return false;

            FP overlapZ = (c1.Size.z + c2.Size.z) * FP.Half - TSMath.Abs(diff.z);
            if (overlapZ <= 0) return false;

            // 找到最小穿透深度，沿该轴推开
            if (overlapX < overlapY && overlapX < overlapZ)
            {
                normal = new TSVector(diff.x > 0 ? 1 : -1, 0, 0);
                depth = overlapX;
            }
            else if (overlapY < overlapZ)
            {
                normal = new TSVector(0, diff.y > 0 ? 1 : -1, 0);
                depth = overlapY;
            }
            else
            {
                normal = new TSVector(0, 0, diff.z > 0 ? 1 : -1);
                depth = overlapZ;
            }

            return true;
        }

        #endregion
    }
}