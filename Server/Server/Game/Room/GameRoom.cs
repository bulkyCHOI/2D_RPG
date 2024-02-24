using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Map Map { get; private set; } = new Map();
        
        public void Init(int mapId)
        {
            Map.LoadMap(mapId);

            //몬스터 생성
            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.Init(1);    //임시로 1번 몬스터 셋팅
            monster.CellPos = new Vector2Int(5, 5);
            //EnterGame(monster);   //job 방식으로 변경
            Push(EnterGame, monster);   //job 방식으로 변경
        }

        //누군가가 주기적으로 호출해줘야 한다.
        public void Update()
        {
            foreach(Monster monster in _monsters.Values)
            {
                monster.Update();
            }
            foreach (Projectile projectile in _projectiles.Values)
            {
                projectile.Update();
            }

            Flush();
        }

        public void EnterGame(GameObject gameObject)
        {
            if(gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y)); //초기 위치로 이동

                // 본인한테 정보 전송
                { 
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    // 본인한테 다른 플레이어 정보 전송
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if(p != player)  //위에서 한번 전송했으니까
                            spawnPacket.Objects.Add(p.Info);
                    }
                    foreach (Monster m in _monsters.Values)
                        spawnPacket.Objects.Add(m.Info);
                    foreach (Projectile p in _projectiles.Values)
                        spawnPacket.Objects.Add(p.Info);
                    player.Session.Send(spawnPacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;

                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y)); //초기 위치로 이동
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;
            }

            // 다른 플레이어들에게 정보 전송
            { 
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                foreach (Player p in _players.Values)  
                {
                    if (p.Id != gameObject.Id) // 본인한테는 이미 전송했으니까
                        p.Session.Send(spawnPacket);
                }
            }
        }   

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;
                
                player.OnLeaveGame();
                Map.ApplyLeave(player); 
                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = null;
                if (_monsters.Remove(objectId, out monster) == false)
                    return;
                Map.ApplyLeave(monster);
                monster.Room = null;
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if (_projectiles.Remove(objectId, out projectile) == false)
                    return;
                projectile.Room = null;
            }
                
            // 다른 플레이어들에게 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                foreach (Player p in _players.Values)
                {
                    if(p.Id != objectId) // 본인한테는 이미 전송했으니까
                        p.Session.Send(despawnPacket);
                }
            }
        }

        //Push를 사용하여 호출하지 않으면 멀티쓰레드에서 호출할 경우 문제가 발생할 수 있다.
        //하지만 즉각적으로 처리해야하는 경우에는 Push를 사용하지 않는다.
        //FindPlayer, Broadcast는 Push를 사용하지 않는다.
        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player p in _players.Values)
            {
                if (condition.Invoke(p))
                    return p;
            }
            return null;
        }

        public void Broadcast(IMessage packet)
        {
            foreach (Player p in _players.Values)
            {
                p.Session.Send(packet);
            }
        }
    }
}
