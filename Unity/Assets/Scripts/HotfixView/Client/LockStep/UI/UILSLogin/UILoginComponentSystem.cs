using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 帧同步-Login系统
    /// </summary>
    [EntitySystemOf(typeof(UILSLoginComponent))]
    [FriendOf(typeof(UILoginComponent))]
    [FriendOfAttribute(typeof(ET.Client.UILSLoginComponent))]
    public static partial class UILSLoginComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILSLoginComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            self.loginBtn = rc.Get<GameObject>("LoginBtn");

            self.loginBtn.GetComponent<Button>().onClick.AddListener(() => { self.OnLogin(); });
            self.account = rc.Get<GameObject>("Account");
            self.password = rc.Get<GameObject>("Password");
        }


        public static void OnLogin(this UILSLoginComponent self)
        {
            Log.Info($"pxq--帧同步--点击Login按钮后-开始登录--account:{self.account}, password:{self.password}--");
            LoginHelper.Login(
                self.Root(),
                self.account.GetComponent<InputField>().text,
                self.password.GetComponent<InputField>().text).Coroutine();
        }
    }
}
