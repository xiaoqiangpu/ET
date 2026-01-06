using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    /// <summary>
    /// 客户端到网关的登录流程逻辑
    /// </summary>
    [MessageHandler(SceneType.NetClient)]
    public class Main2NetClient_LoginHandler: MessageHandler<Scene, Main2NetClient_Login, NetClient2Main_Login>
    {
        protected override async ETTask Run(Scene root, Main2NetClient_Login request, NetClient2Main_Login response)
        {
            string account = request.Account;
            string password = request.Password;
            
            Log.Info($"pxq--Main2NetClient_LoginHandler--1--account:{account}--pwd:{password}--RouterHttpHost:{ConstValue.RouterHttpHost}--RouterHttpPort:{ConstValue.RouterHttpPort}");
            
            // 创建一个ETModel层的Session
            root.RemoveComponent<RouterAddressComponent>();
            // 获取路由跟realmDispatcher地址
            RouterAddressComponent routerAddressComponent =
                    root.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
            await routerAddressComponent.Init();
            
            Log.Info($"pxq--Main2NetClient_LoginHandler--2--获取Router地址：{routerAddressComponent.ToJson()}");
            
            root.AddComponent<NetComponent, AddressFamily, NetworkProtocol>(routerAddressComponent.RouterManagerIPAddress.AddressFamily, NetworkProtocol.UDP);
            root.GetComponent<FiberParentComponent>().ParentFiberId = request.OwnerFiberId;

            NetComponent netComponent = root.GetComponent<NetComponent>();
            IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);
            
            Log.Info($"pxq--Main2NetClient_LoginHandler--3--Realm网关负载均衡器地址--address:{realmAddress.Address}--port:{realmAddress.Port}");
            
            R2C_Login r2CLogin;
            using (Session session = await netComponent.CreateRouterSession(realmAddress, account, password))
            {
                C2R_Login c2RLogin = C2R_Login.Create();
                c2RLogin.Account = account;
                c2RLogin.Password = password;
                r2CLogin = (R2C_Login)await session.Call(c2RLogin);
            }
            Log.Info($"pxq--Main2NetClient_LoginHandler--4--获取Realm分配的网关地址信息--gate--address:{r2CLogin.Address}--gate key:{r2CLogin.Key}--gate Id:{r2CLogin.GateId}");
            
            // 创建一个gate Session,并且保存到SessionComponent中
            Session gateSession = await netComponent.CreateRouterSession(NetworkHelper.ToIPEndPoint(r2CLogin.Address), account, password);
            gateSession.AddComponent<ClientSessionErrorComponent>();
            root.AddComponent<SessionComponent>().Session = gateSession;
            C2G_LoginGate c2GLoginGate = C2G_LoginGate.Create();
            c2GLoginGate.Key = r2CLogin.Key;
            c2GLoginGate.GateId = r2CLogin.GateId;
            G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(c2GLoginGate);
            
            Log.Info($"pxq--Main2NetClient_LoginHandler--5--创建客户端到网关服务的连接Session--RpcId:{g2CLoginGate.RpcId}--playerId:{g2CLoginGate.PlayerId}--");
            
            Log.Debug("登陆gate成功!");

            response.PlayerId = g2CLoginGate.PlayerId;
        }
    }
}