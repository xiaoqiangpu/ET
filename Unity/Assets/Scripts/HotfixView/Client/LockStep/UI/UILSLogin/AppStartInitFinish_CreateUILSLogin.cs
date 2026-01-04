namespace ET.Client
{
	/// <summary>
	/// 帧同步-App开始创建Login UI
	/// </summary>
	[Event(SceneType.LockStep)]
	public class AppStartInitFinish_CreateUILSLogin: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			await UIHelper.Create(root, UIType.UILSLogin, UILayer.Mid);
		}
	}
}
