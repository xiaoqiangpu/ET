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
            LSUnit    lsUnit   = self.GetParent<LSUnit>();
            LSRigidBody rb = lsUnit.GetComponent<LSRigidBody>();
            if(rb==null) return;
            
            Log.Info($"pxq--LSInputComponentSystem-LSUpdate-1-lsUnit Id:{lsUnit.Id}--LsUnit Pos:{self.LSInput.V.ToString()}");
   
            TSVector2 inputDir = self.LSInput.V;
            FP        speed    = 6; 
            Log.Info($"pxq--LSInputComponentSystem-LSUpdate-2--LSUpdate--lsUnit Id:{lsUnit.Id}--LsUnit Pos:{self.LSInput.V.ToString()}--inputDir.LengthSquared()：{inputDir.LengthSquared().ToString()}");
            if (inputDir.LengthSquared() > 0.0001f)
            {
                //只修改水平速度 (Velocity X/Z),Velocity.Y (重力在管)
                rb.Velocity.x = inputDir.x * speed;
                rb.Velocity.z = inputDir.y * speed;
                // 更新朝向
                lsUnit.Forward = new TSVector(inputDir.x, 0, inputDir.y);
                Log.Info($"pxq--LSInputComponentSystem-LSUpdate-3--Update RigidBody Velocity XZ--lsUnit Id:{lsUnit.Id}--LsUnit Pos:{self.LSInput.V.ToString()}--inputDir.LengthSquared()：{inputDir.LengthSquared().ToString()}");
            }
            else
            {
                // 没输入就停下
                rb.Velocity.x = 0;
                rb.Velocity.z = 0;
            }
        }
        
        #endregion
    }
}