using System;


namespace ET.Server
{
	/// <summary>
	/// Realm负载均衡服务器向Gate网关服务器请求Login Key
	/// </summary>
	[MessageHandler(SceneType.Gate)]
	public class R2G_GetLoginKeyHandler : MessageHandler<Scene, R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override async ETTask Run(Scene scene, R2G_GetLoginKey request, G2R_GetLoginKey response)
		{
			long key = RandomGenerator.RandInt64();
			scene.GetComponent<GateSessionKeyComponent>().Add(key, request.Account);
			response.Key = key;
			response.GateId = scene.Id;
			await ETTask.CompletedTask;
		}
	}
}