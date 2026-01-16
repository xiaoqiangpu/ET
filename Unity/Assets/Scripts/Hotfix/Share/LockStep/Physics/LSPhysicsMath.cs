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

            // 1. 球 vs 球
            if (c1.ShapeType == LSColliderType.Sphere && c2.ShapeType == LSColliderType.Sphere)
            {
                return SphereVsSphere(c1.WorldCenter, c1.Radius, c2.WorldCenter, c2.Radius, out normal, out depth);
            }

            // 2. 球 vs 盒
            if (c1.ShapeType == LSColliderType.Sphere && c2.ShapeType == LSColliderType.Box)
            {
                return SphereVsBox(c1.WorldCenter, c1.Radius, c2.WorldCenter, c2.Size, out normal, out depth);
            }

            if (c1.ShapeType == LSColliderType.Box && c2.ShapeType == LSColliderType.Sphere)
            {
                bool hit = SphereVsBox(c2.WorldCenter, c2.Radius, c1.WorldCenter, c1.Size, out normal, out depth);
                normal = -1 * normal; // 反转法线
                return hit;
            }

            // 3. 盒 vs 盒 (简化版：仅处理无旋转 AABB)
            if (c1.ShapeType == LSColliderType.Box && c2.ShapeType == LSColliderType.Box)
            {
                return AABBVsAABB(c1, c2, out normal, out depth);
            }

            
            //TODO: 胶囊体数学较复杂，通常拆解为 线段 vs 图形。这里为了篇幅暂略，临时用 Sphere 近似代替----后续待完善--pxq
            //4.胶囊 vs 胶囊
            if (c1.ShapeType == LSColliderType.Capsule && c2.ShapeType == LSColliderType.Capsule)
            {
                return SphereVsSphere(c1.WorldCenter, c1.Radius, c2.WorldCenter, c2.Radius, out normal, out depth);
            }
            //5.胶囊 vs 球
            if (c1.ShapeType == LSColliderType.Capsule && c2.ShapeType == LSColliderType.Sphere)
            {
                return SphereVsSphere(c1.WorldCenter, c1.Radius, c2.WorldCenter, c2.Radius, out normal, out depth);
            }
            
            if (c1.ShapeType == LSColliderType.Sphere && c2.ShapeType == LSColliderType.Capsule)
            {
                return SphereVsSphere(c2.WorldCenter, c2.Radius, c1.WorldCenter, c1.Radius, out normal, out depth);
            }
            
            //6.胶囊 vs 盒
            if (c1.ShapeType == LSColliderType.Capsule && c2.ShapeType == LSColliderType.Box)
            {
                return SphereVsBox(c1.WorldCenter, c1.Radius, c2.WorldCenter, c2.Size, out normal, out depth);
            }
            
            if (c1.ShapeType == LSColliderType.Box && c2.ShapeType == LSColliderType.Capsule)
            {
                return SphereVsBox(c2.WorldCenter, c2.Radius, c1.WorldCenter, c1.Size, out normal, out depth);
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

            TSVector halfSize = boxSize * FP.Half;

            // 1. 将圆心转换到 Box 局部坐标 (假设 Box 无旋转)
            TSVector localPos = sphereCenter - boxCenter;

            // 2. 寻找 Box 表面距离圆心最近的点 (Clamped Point)
            TSVector closest = localPos;
            closest.x = TSMath.Clamp(closest.x, -halfSize.x, halfSize.x);
            closest.y = TSMath.Clamp(closest.y, -halfSize.y, halfSize.y);
            closest.z = TSMath.Clamp(closest.z, -halfSize.z, halfSize.z);

            // 3. 计算距离
            TSVector diff   = localPos - closest;
            FP       distSq = diff.sqrMagnitude;

            if (distSq > radius * radius) return false;

            FP dist = TSMath.Sqrt(distSq);

            // 特殊情况：圆心在 Box 内部 (dist == 0)
            if (dist == 0)
            {
                // 简单策略：找到离哪个面最近，就往哪推
                // (这里省略具体判断，简单往 Y 轴推，实际需要判断6个面)
                normal = TSVector.up;
                depth = radius + halfSize.y;
                return true;
            }

            normal = diff / dist;
            depth = radius - dist;
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