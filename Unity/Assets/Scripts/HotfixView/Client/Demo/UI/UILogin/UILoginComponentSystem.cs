using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[EntitySystemOf(typeof(UILoginComponent))]
	[FriendOf(typeof(UILoginComponent))]
	public static partial class UILoginComponentSystem
	{
		[EntitySystem]
		private static void Awake(this UILoginComponent self)
		{
			ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
			self.loginBtn = rc.Get<GameObject>("LoginBtn");
			
			self.loginBtn.GetComponent<Button>().onClick.AddListener(()=> { self.OnLogin(); });
			self.account = rc.Get<GameObject>("Account");
			self.password = rc.Get<GameObject>("Password");
		}

		
		public static void OnLogin(this UILoginComponent self)
		{
			Log.Info($"pxq--点击Login按钮后-开始登录--account:{self.account}, password:{self.password}--");
			LoginHelper.Login(
				self.Root(), 
				self.account.GetComponent<InputField>().text, 
				self.password.GetComponent<InputField>().text).Coroutine();
		}
	}
}
