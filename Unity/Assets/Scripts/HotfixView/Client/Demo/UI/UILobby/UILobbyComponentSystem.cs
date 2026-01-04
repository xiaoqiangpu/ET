using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// UI-大厅（匹配）组件系统
    /// </summary>
    [EntitySystemOf(typeof(UILobbyComponent))]
    [FriendOf(typeof(UILobbyComponent))]
    public static partial class UILobbyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILobbyComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            self.enterMap = rc.Get<GameObject>("EnterMap");
            self.enterMap.GetComponent<Button>().onClick.AddListener(() => { self.EnterMap().Coroutine(); });
        }
        
        public static async ETTask EnterMap(this UILobbyComponent self)
        {
            Scene root = self.Root();
            Log.Info($"pxq--点击大厅匹配--开始进入地图");
            await EnterMapHelper.EnterMapAsync(root);
            await UIHelper.Remove(root, UIType.UILobby);
        }
    }
}