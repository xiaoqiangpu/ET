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
            LSUnit unit = self.GetUnit();
            if (unit != null)
            {
                Vector3 initPos = unit.Position.ToVector(); // 扩展方法: TSVector -> Vector3
                self.Transform.position = initPos;
                self.Position = initPos; // 更新缓存
                
                if (unit.Forward != TSVector.zero)
                {
                    self.Transform.rotation = Quaternion.LookRotation(unit.Forward.ToVector());
                }
            }

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

             LSUnit unit = self.GetUnit();
            if (unit == null || unit.IsDisposed) return;

            // ============================================================
            // 1. 获取权威数据 (逻辑层数据)
            // ============================================================
            // 将定点数转为 Unity 浮点数
            // 建议封装 ToVector() 扩展方法，或者手动 new Vector3(x.AsFloat()...)
            Vector3 targetPos = new Vector3(unit.Position.x.AsFloat(), unit.Position.y.AsFloat(), unit.Position.z.AsFloat());
            Vector3 targetFwd = new Vector3(unit.Forward.x.AsFloat(), unit.Forward.y.AsFloat(), unit.Forward.z.AsFloat());

            // ============================================================
            // 2. 位置同步 (替换掉了原来的 totalTime/speed2 逻辑)
            // ============================================================
            float distance = Vector3.Distance(self.Transform.position, targetPos);

            // [防抖动阈值]
            // 如果距离极小（比如浮点误差），就不动了，防止静止时微弱抖动
            if (distance < 0.01f)
            {
                // do nothing or snap exact
            }
            // [瞬移阈值]
            // 如果距离过大（超过2米，说明可能是传送、出生、或严重回滚），直接瞬移，不要插值
            else if (distance > 2.0f)
            {
                self.Transform.position = targetPos;
                self.Position = targetPos; // 更新缓存
            }
            // [平滑跟随]
            // 使用 Lerp 进行平滑过渡。15f 是跟随硬度，值越大跟得越紧，物理感越弱。
            else
            {
                self.Transform.position = Vector3.Lerp(self.Transform.position, targetPos, Time.deltaTime * 15f);
                self.Position = targetPos; // 更新缓存
            }

            // ============================================================
            // 3. 旋转同步
            // ============================================================
            if (targetFwd != Vector3.zero) // 防止零向量报错
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetFwd);
                // 使用 Slerp 平滑旋转
                self.Transform.rotation = Quaternion.Slerp(self.Transform.rotation, targetRotation, Time.deltaTime * 20f);
                self.Rotation = targetRotation; // 更新缓存
            }

            // ============================================================
            // 4. 动画同步 (改为基于物理速度)
            // ============================================================
            UpdateAnimation(self, unit);

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

            self.Unit = (self.IScene as Room).LSWorld.GetComponent<LSUnitComponent>().GetChild<LSUnit>(self.Id);
            return self.Unit;
        }
    }
}