using System;
using TrueSync;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Unit View层控制逻辑
    /// </summary>
    [EntitySystemOf(typeof(LSUnitView))]
    [LSEntitySystemOf(typeof(LSUnitView))]
    [FriendOf(typeof(LSUnitView))]
    [FriendOf(typeof(LSRigidBody))]
    public static partial class LSUnitViewSystem
    {
        [EntitySystem]
        private static void Awake(this LSUnitView self, GameObject go)
        {
            self.GameObject = go;
            self.Transform = go.transform;

            #region 优化调整

            // [优化] 初始化时直接同步一次位置，防止 GameObject 从 (0,0,0) 飞过来
            // LSUnit unit = self.GetUnit();
            // if (unit != null)
            // {
            //     Vector3 initPos = unit.Position.ToVector(); // 扩展方法: TSVector -> Vector3
            //     self.Transform.position = initPos;
            //     self.Position = initPos; // 更新缓存
            //
            //     if (unit.Forward != TSVector.zero)
            //     {
            //         self.Transform.rotation = Quaternion.LookRotation(unit.Forward.ToVector());
            //     }
            // }

            #endregion
        }

        [LSEntitySystem]
        private static void LSRollback(this LSUnitView self)
        {
            //LSUnit unit = self.GetUnit();
            //self.Transform.position = unit.Position.ToVector();
            //self.Transform.rotation = unit.Rotation.ToQuaternion();
            //self.t = 0;
            //self.totalTime = 0;
        }

        [EntitySystem]
        private static void Update(this LSUnitView self)
        {
            #region OldCode

            // LSUnit unit = self.GetUnit();
            //
            // Vector3     unitPos = unit.Position.ToVector();
            // const float speed   = 6f;
            // float       speed2  = speed; // * self.Room().SpeedMultiply;
            //
            // if (unitPos != self.Position)
            // {
            //     float distance = (unitPos - self.Position).magnitude;
            //     self.totalTime = distance / speed2;
            //     self.t = 0;
            //     self.Position = unit.Position.ToVector();
            //     self.Rotation = unit.Rotation.ToQuaternion();
            // }
            //
            // LSInput input = unit.GetComponent<LSInputComponent>().LSInput;
            // if (input.V != TSVector2.zero)
            // {
            //     self.GetComponent<LSAnimatorComponent>().SetFloatValue("Speed", speed2);
            // }
            // else
            // {
            //     self.GetComponent<LSAnimatorComponent>().SetFloatValue("Speed", 0);
            // }
            //
            // self.t += Time.deltaTime;
            // self.Transform.rotation = Quaternion.Lerp(self.Transform.rotation, self.Rotation, self.t / 1f);
            // self.Transform.position = Vector3.Lerp(self.Transform.position, self.Position, self.t / self.totalTime);

            #endregion

            #region NewCode

            LSUnit lsUnit = self.GetUnit();
            if (lsUnit == null) return;

            // 1. 获取权威位置
            Vector3 targetPos = lsUnit.Position.ToVector();
            Vector3 targetFwd = lsUnit.Forward.ToVector();

            // 2. 位置同步 (Lerp)
            float dist = Vector3.Distance(self.Transform.position, targetPos);
    
            // 如果误差太大（>2米），瞬移纠正
            if (dist > 2f) 
            {
                self.Transform.position = targetPos;
            }
            else 
            {
                // 平滑跟随
                self.Transform.position = Vector3.Lerp(self.Transform.position, targetPos, Time.deltaTime * 15f);
            }

            // 3. 旋转同步
            if (targetFwd != Vector3.zero)
            {
                self.Transform.rotation = Quaternion.Slerp(self.Transform.rotation, Quaternion.LookRotation(targetFwd), Time.deltaTime * 20f);
            }
            // 4. 动画同步 (改为基于物理速度)
            UpdateAnimation(self, lsUnit);

            #endregion
        }

        private static void UpdateAnimation(LSUnitView self, LSUnit unit)
        {
            var animator = self.GetComponent<LSAnimatorComponent>();
            if (animator == null) return;

            // [优化] 不再读取 Input，而是读取刚体真实速度
            // 这样即使玩家没按键，如果被击飞或滑行，也能正确处理（或者切换到由于惯性移动的动作）
            LSRigidBody rb    = unit.GetComponent<LSRigidBody>();
            float       speed = 0f;

            if (rb != null)
            {
                // 计算水平速度 (忽略 Y 轴，避免下落时播放跑步动作)
                float vx = rb.Velocity.x.AsFloat();
                float vz = rb.Velocity.z.AsFloat();
                speed = Mathf.Sqrt(vx * vx + vz * vz);
            }
            else
            {
                // 兼容没有刚体的情况，回退到 Input 判断 (原有逻辑)
                // LSInput input = unit.GetComponent<LSInputComponent>()?.LSInput;
                // if (input != null && input.V != TSVector2.zero)
                // {
                //     speed = 6f; // 默认跑步速度
                // }
            }

            animator.SetFloatValue("Speed", speed);
        }

        private static LSUnit GetUnit(this LSUnitView self)
        {
            LSUnit unit = self.Unit;
            if (unit != null)
            {
                return unit;
            }
            LSUnit lsUnit= (self.IScene as Room).LSWorld.GetComponent<LSUnitComponent>().GetChild<LSUnit>(self.Id);
            self.Unit = lsUnit;
            Log.Info($"pxq--LSUnitViewSystem--GetUnit--InstanceId:{lsUnit.InstanceId}");
            return self.Unit;
        }
    }
}