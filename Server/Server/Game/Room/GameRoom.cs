using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public const int VisionCells = 10;

        public int monsterCount = 20;
        public int monsterNumber = 1;
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
        Dictionary<int, NPC> _npcs = new Dictionary<int, NPC>();

        public Zone[,] Zones { get; private set; }
        public int ZoneCells { get; private set; }

        public Map Map { get; private set; } = new Map();
        
        public Zone GetZone(Vector2Int cellPos)
        {
            int x = (cellPos.x - Map.MinX) / ZoneCells;
            int y = (Map.MaxY - cellPos.y) / ZoneCells;

            return GetZone(y, x);
        }

        public Zone GetZone(int y, int x)
        {
            if (x < 0 || x >= Zones.GetLength(1))
                return null;
            if (y < 0 || y >= Zones.GetLength(0))
                return null;

            return Zones[y, x];
        }

        public void Init(int mapId, int zoneCells)
        {
            Map.LoadMap(mapId);

            //Zone
            ZoneCells = zoneCells;
            int countY = (Map.SizeY - 1) / ZoneCells + 1;
            int countX = (Map.SizeX - 1) / ZoneCells + 1;
            Zones = new Zone[countY, countX];
            for (int y = 0; y < countY; y++)
            {
                for (int x = 0; x < countX; x++)
                {
                    Zones[y, x] = new Zone(y, x);
                }
            }

            //몬스터 생성
            for (int i = 0; i < monsterCount; i++)
            {
                Monster monster = ObjectManager.Instance.Add<Monster>();
                monster.Init(monsterNumber);    //임시로 1번 몬스터 셋팅
                if(i%5==4)
                    monster.Init(monsterNumber+10);    //5마리당 1마리 꼴로 상위 몬스터 생성
                //EnterGame(monster);   //job 방식으로 변경
                Push(EnterGame, monster, true);   //job 방식으로 변경
            }

            //Console.WriteLine($"mapid: {mapId}");
            if (mapId == 2) //
            {
                //NPC 생성
                for (int i = 0; i < 3; i++)
                {
                    NPC npc = ObjectManager.Instance.Add<NPC>();
                    npc.Init(i+1);    //2번 상인(포션)
                    if (i == 0)
                    {
                        //npc.VendorType = VendorType.Potion;
                        npc.CellPos = new Vector2Int(15, 2);
                    }
                    else if (i == 1)
                    {
                        //npc.VendorType = VendorType.Grocer;
                        npc.CellPos = new Vector2Int(3, 2);
                    }
                    else if (i == 2)
                    {
                        //npc.VendorType = VendorType.Blacksmith;
                        npc.CellPos = new Vector2Int(15, 10);
                    }
                    //EnterGame(npc);   //job 방식으로 변경
                    Push(EnterGame, npc, false);   //job 방식으로 변경
                }
            }
        }

        //누군가가 주기적으로 호출해줘야 한다.
        public void Update()
        {
            Flush();
        }

        Random _rand = new Random();
        public void EnterGame(GameObject gameObject, bool randPos)
        {
            if(gameObject == null)
                return;

            if(randPos) //랜덤한 위치에 생성
            {
                Vector2Int respawnPos;
                while (true)
                {
                    respawnPos.x = _rand.Next(Map.MinX, Map.MaxX + 1);
                    respawnPos.y = _rand.Next(Map.MinY, Map.MaxY + 1);
                    if(Map.CanGo(respawnPos) == false)  //이동불가 지역에 생성되지 않도록
                        continue;
                    if (Map.Find(respawnPos) == null)   //
                    {
                        gameObject.CellPos = respawnPos;
                        break;
                    }
                }
            }

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);
            //Console.WriteLine($"Enter Room({RoomId}): {Map.mapName}({Map.MinX},{Map.MaxX}) - [{type}]{gameObject.Id}");

            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                player.RefreshAdditionalStat();

                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y)); //초기 위치로 이동
                GetZone(player.CellPos).Players.Add(player);    //zone에 추가

                // 본인한테 정보 전송
                { 
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);
                    player.Vision.Update();
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;

                GetZone(monster.CellPos).Monsters.Add(monster);    //zone에 추가
                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y)); //초기 위치로 이동

                monster.Update();   //job 방식으로 변경 //몬스터에 대한 update를 1회 호출하고 그 후에는 재귀적으로 호출한다.
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;

                GetZone(projectile.CellPos).Projectiles.Add(projectile);    //zone에 추가
                projectile.Update();    //job 방식으로 변경 //투사체에 대한 update를 1회 호출하고 그 후에는 재귀적으로 호출한다.
            }
            else if(type == GameObjectType.Npc)
            {
                //Console.WriteLine("NPC create");
                NPC npc = gameObject as NPC;
                _npcs.Add(gameObject.Id, npc);
                npc.Room = this;

                GetZone(npc.CellPos).NPCs.Add(npc);    //zone에 추가
                Map.ApplyMove(npc, new Vector2Int(npc.CellPos.x, npc.CellPos.y)); //초기 위치로 이동
            }
            else
            {
                return;
            }
            //타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                Broadcast(gameObject.CellPos, spawnPacket);
            }
        }   

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            Vector2Int cellPos;
            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                cellPos = player.CellPos;
                
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

                cellPos = monster.CellPos;
                Map.ApplyLeave(monster);
                monster.Room = null;
            }
            else   if (type == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if (_projectiles.Remove(objectId, out projectile) == false)
                    return;
                
                cellPos = projectile.CellPos;
                Map.ApplyLeave(projectile);
                projectile.Room = null;
            }
            else
            {
                return;
            }
            
            //타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                Broadcast(cellPos, despawnPacket);
            }

        }

        //Push를 사용하여 호출하지 않으면 멀티쓰레드에서 호출할 경우 문제가 발생할 수 있다.
        //하지만 즉각적으로 처리해야하는 경우에는 Push를 사용하지 않는다.
        //FindPlayer, Broadcast는 Push를 사용하지 않는다.
        Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player p in _players.Values)
            {
                if (condition.Invoke(p))
                    return p;
            }
            return null;
        }

        //살짝 부담스러운 함수   
        public Player FindClosestPlayer(Vector2Int pos, int range)
        {
            List<Player> players = GetAdjacentPlayers(pos, range);
            players.Sort((a, b) =>
            {
                int leftDist = (a.CellPos - pos).cellDistFromZero;
                int rightDist = (b.CellPos - pos).cellDistFromZero;
                return leftDist - rightDist;
            });

            foreach (Player player in players)
            {
                List<Vector2Int> path = Map.FindPath(pos, player.CellPos, checkObjects: true);
                if (path.Count < 2 || path.Count > range)
                    continue;

                return player;
            }

            return null;
        }

        public void Broadcast(Vector2Int pos, IMessage packet)
        {
            List<Zone> zones = GetAdjacentZones(pos);
            //foreach (Zone zone in zones)
            //{
            //    foreach (Player p in zone.Players)
            //    {
            //        p.Session.Send(packet);
            //    }
            //}
            // 이중 foreach문을 LINQ로 변경
            foreach(Player p in zones.SelectMany(z => z.Players))
            {
                //zoned에 있는 모든 플레이어가 아닌 VisionCells안에 잇는 플레이어에게만 전송
                int dx = p.CellPos.x - pos.x;
                int dy = p.CellPos.y - pos.y;
                if (Math.Abs(dx) > VisionCells || Math.Abs(dy) > VisionCells)
                    continue;

                p.Session.Send(packet);
            }
        }

        public List<Player> GetAdjacentPlayers(Vector2Int cellPos, int range)   //주변 존내의 플레이어 목록을 반환
        {
            List<Zone> zones = GetAdjacentZones(cellPos, range);
            return zones.SelectMany(z => z.Players).ToList();
        }

        public List<Zone> GetAdjacentZones(Vector2Int cellPos, int range = GameRoom.VisionCells)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int maxY = cellPos.y + range;
            int minY = cellPos.y - range;
            int maxX = cellPos.x + range;
            int minX = cellPos.x - range;

            //좌상단
            Vector2Int leftTop = new Vector2Int(minX, maxY);
            int minIndexY = (Map.MaxY - leftTop.y) / ZoneCells;
            int minIndexX = (leftTop.x - Map.MinX) / ZoneCells;
            //우하단
            Vector2Int rightBottom = new Vector2Int(maxX, minY);
            int maxIndexY = (Map.MaxY - rightBottom.y) / ZoneCells;
            int maxIndexX = (rightBottom.x - Map.MinX) / ZoneCells;

            for (int x = minIndexX; x <= maxIndexX; x++)
            {
                for (int y = minIndexY; y <= maxIndexY; y++)
                {
                    Zone zone = GetZone(y, x);
                    if (zone == null)
                        continue;
                    
                    zones.Add(zone);
                }
            }
                        
            return zones.ToList();
        }
    }
}
