using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Zone
    {
        public int IndexY { get; private set; }
        public int IndexX { get; private set; }

        public HashSet<Player> Players { get; set; } = new HashSet<Player>();
        public HashSet<Monster> Monsters { get; set; } = new HashSet<Monster>();
        public HashSet<Projectile> Projectiles { get; set; } = new HashSet<Projectile>();
        public HashSet<NPC> NPCs { get; set; } = new HashSet<NPC>();
        public HashSet<DropItem> DropItems { get; set; } = new HashSet<DropItem>();


        public Zone(int y, int x)
        {
            IndexY = y;
            IndexX = x;
        }

        public void Remove(GameObject gameObject)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            switch (type)
            {
                case GameObjectType.Player:
                    Players.Remove((Player)gameObject);
                    break;
                case GameObjectType.Monster:
                    Monsters.Remove((Monster)gameObject);
                    break;
                case GameObjectType.Projectile:
                    Projectiles.Remove((Projectile)gameObject);
                    break;
                case GameObjectType.Npc:
                    NPCs.Remove((NPC)gameObject);
                    break;
                case GameObjectType.Item:
                    DropItems.Remove((DropItem)gameObject);
                    break;
            }
        }   

        public Player FindOnePlayer(Func<Player, bool> condition)
        {
            foreach (Player player in Players)
            {
                if (condition.Invoke(player))
                    return player;
            }
            return null;
        }

        public List<Player> FindAllPlayer(Func<Player, bool> condition)
        {
            List<Player> findList = new List<Player>();
            foreach (Player player in Players)
            {
                if (condition.Invoke(player))
                    findList.Add(player);
            }
            return findList;
        }
    }
}
