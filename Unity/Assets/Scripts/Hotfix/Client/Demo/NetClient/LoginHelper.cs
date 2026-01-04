namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene root, string account, string password)
        {
            root.RemoveComponent<ClientSenderComponent>();
            
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            
            long playerId = await clientSenderComponent.LoginAsync(account, password);

            root.GetComponent<PlayerComponent>().MyId = playerId;
            Log.Info($"pxq--Login--Finish--sceneType:{root.SceneType.ToString()}--playerId:{playerId}--publish LoginFinish--");
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}