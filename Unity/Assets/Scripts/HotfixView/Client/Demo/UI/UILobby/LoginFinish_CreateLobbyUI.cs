namespace ET.Client
{
	/// <summary>
	/// 登录完成-创建大厅UI
	/// </summary>
	[Event(SceneType.Demo)]
	public class LoginFinish_CreateLobbyUI: AEvent<Scene, LoginFinish>
	{
		protected override async ETTask Run(Scene scene, LoginFinish args)
		{
			await UIHelper.Create(scene, UIType.UILobby, UILayer.Mid);
		}
	}
}
