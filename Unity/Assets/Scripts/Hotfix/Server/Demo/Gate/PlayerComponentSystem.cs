using System.Linq;

namespace ET.Server
{
    /// <summary>
    /// PlayerComponent系统
    /// 增加/移除、获取玩家逻辑
    /// </summary>
    [FriendOf(typeof(PlayerComponent))]
    public static partial class PlayerComponentSystem
    {
        public static void Add(this PlayerComponent self, Player player)
        {
            self.dictionary.Add(player.Account, player);
        }
        
        public static void Remove(this PlayerComponent self, Player player)
        {
            self.dictionary.Remove(player.Account);
            player.Dispose();
        }
        
        public static Player GetByAccount(this PlayerComponent self,  string account)
        {
            self.dictionary.TryGetValue(account, out EntityRef<Player> player);
            return player;
        }
    }
}