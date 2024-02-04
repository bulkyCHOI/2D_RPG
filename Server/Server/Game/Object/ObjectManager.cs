using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();
        object _lock = new object();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        // [UNUSED(1)][TYPE(7)][ID(24)]
        // [........|........|........|........]
        int _counter = 1; //TODO - 쪼개서 다른 정보들도 넣자

        public T Add<T>() where T : GameObject, new()
        {
            T obj = new T();

            lock (_lock)
            {
                obj.Id = GenerateId(obj.ObjectType);

                if (obj.ObjectType == GameObjectType.Player)
                    _players.Add(obj.Id, obj as Player);
            }

            return obj;
        }

        int GenerateId(GameObjectType type)
        {
            lock (_lock)
            {
                return ((int)type << 24) | (_counter++);
            }
        }

        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        public bool Remove(int objectId)
        {
            GameObjectType type = GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (type == GameObjectType.Player)
                    return _players.Remove(objectId);
            }
            return false;
        }

        public Player Find(int objectId)
        {
            GameObjectType type = GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player player = null;

                    if (_players.TryGetValue(objectId, out player))
                        return player;
                }
            }
            return null;
        }
    }
}
