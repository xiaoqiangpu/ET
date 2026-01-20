using System;
using ET.Client;
using TrueSync;

namespace ET
{
    [EntitySystemOf(typeof(LSInputComponent))]
    [LSEntitySystemOf(typeof(LSInputComponent))]
    [FriendOf(typeof(LSRigidBody))]
    public static partial class LSInputComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSInputComponent self)
        {

        }

        #region 没有物理组件的LSUpdate

        // [LSEntitySystem]
        // private static void LSUpdate(this LSInputComponent self)
        // {
        //     LSUnit unit = self.GetParent<LSUnit>();
        //
        //     TSVector2 v2 = self.LSInput.V * 6 * 50 / 1000;
        //     if (v2.LengthSquared() < 0.0001f)
        //     {
        //         return;
        //     }
        //     TSVector oldPos = unit.Position;
        //     unit.Position += new TSVector(v2.x, 0, v2.y);
        //     unit.Forward = unit.Position - oldPos;
        // }

        #endregion


        #region 有物理组件的LSUpdate
        
        [LSEntitySystem]
        private static void LSUpdate(this LSInputComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();
            
            // 1. 获取刚体组件
            // 如果没有刚体，说明这个单位不受物理控制（可能是无敌状态或特殊剧情），直接返回或走老逻辑
            LSRigidBody rb = unit.GetComponent<LSRigidBody>();
            if (rb == null) return;

            // 2. 获取输入向量 (TSVector2)
            TSVector2 inputDir = self.LSInput.V;

            // 3. 定义移动速度 (米/秒)
            // 注意：这里不需要乘以时间(50/1000)，因为Velocity是速度，物理引擎积分时会乘以时间
            FP speed = 6; 

            // 4. 应用速度到刚体
            if (inputDir.LengthSquared() > 0.0001f) // 有输入
            {
                // 将 2D 输入转换为 3D 速度向量 (x, 0, y)
                // 注意：我们只控制水平移动，Y轴速度(rb.Velocity.y)交给重力控制，不要覆盖它
                rb.Velocity.x = inputDir.x * speed;
                rb.Velocity.z = inputDir.y * speed;
                
                // 5. 更新朝向 (Forward)
                // 朝向不涉及物理碰撞，可以直接设置
                unit.Forward = new TSVector(inputDir.x, 0, inputDir.y);
            }
            else // 无输入
            {
                // 立即停止水平移动
                // 如果想要"惯性滑行"效果，这里可以不归零，而是让物理引擎的 Drag (阻力) 慢慢减速
                rb.Velocity.x = 0;
                rb.Velocity.z = 0;
            }
            
            // 【重要】
            // 绝对不要在这里写 unit.Position += ... 
            // 这一步将由 LSPhysicsWorldSystem 接管
        }
        
        #endregion
    }
}