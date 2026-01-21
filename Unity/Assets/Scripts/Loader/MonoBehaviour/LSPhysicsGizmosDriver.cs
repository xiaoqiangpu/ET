using System;
using UnityEngine;

namespace ET.Client
{
    // 这个脚本挂在 Unity 场景物体上，作为 ET 逻辑层画线的入口
    public class LSPhysicsGizmosDriver : MonoBehaviour
    {
        // 静态委托，供 ET Hotfix 层注册绘制逻辑
        public static Action OnDrawGizmosCallback;

        private void OnDrawGizmos()
        {
            // 当 Unity 渲染 Gizmos 时，调用这个委托
            if (OnDrawGizmosCallback != null)
            {
                OnDrawGizmosCallback.Invoke();
            }
        }
    }
}