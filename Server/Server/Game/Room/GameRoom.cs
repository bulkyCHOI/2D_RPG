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
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Map Map { get; private set; } = new Map();
        public void Init(int mapId)
        {
            Map.LoadMap(mapId);
        }

        public void Update()
        {
            lock(_lock)
            {
                foreach (Projectile projectile in _projectiles.Values)
                {
                    projectile.Update();
                }
            }
        }

        public void EnterGame(GameObject gameObject)
        {
            if(gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            lock (_lock)
            {
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
        }
        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player player = null;
                    if (_players.Remove(objectId, out player) == false)
                        return;
                    player.Room = null;
                    Map.ApplyLeave(player); 

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
                    monster.Room = null;
                    Map.ApplyLeave(monster);
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
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;
            lock (_lock)
            {
                //TODO : 검증
                PositionInfo movePosInfo = movePacket.PosInfo; //가고싶은 좌표
                ObjectInfo info = player.Info; // 플레이어 정보
                
                //다른좌표로 이동할 경우, 갈수 있는지 체크
                if(movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
                {
                    Vector2Int dest = new Vector2Int(movePosInfo.PosX, movePosInfo.PosY);
                    if (Map.CanGo(dest) == false)
                        return;
                }
                info.PosInfo.State = movePosInfo.State;
                info.PosInfo.MoveDir = movePosInfo.MoveDir;
                Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)); //이동


                // 방에 있는 모든 플레이어에게 전송
                S_Move resMove = new S_Move();
                resMove.ObjectId = player.Info.ObjectId;
                resMove.PosInfo = movePacket.PosInfo;

                Broadcast(resMove);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;
            lock (_lock)
            {
                ObjectInfo playerInfo = player.Info; // 플레이어 정보
                if (playerInfo.PosInfo.State != CreatureState.Idle)  //이동중이면 스킬 사용 불가
                    return;

                //TODO : 스킬 사용 가능 여부 검증

                playerInfo.PosInfo.State = CreatureState.Skill;
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = playerInfo.ObjectId;
                skill.Info.SkillId = skillPacket.Info.SkillId;
                Broadcast(skill);

                Data.Skill skillData = null;
                if(DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                    return;
                switch(skillData.skillType)
                {
                    case SkillType.SkillAuto:
                        Vector2Int skillPos = player.GetFrontCellPos(playerInfo.PosInfo.MoveDir);
                        GameObject target = Map.Find(skillPos);
                        if (target != null)
                        {
                            Console.WriteLine("Hit GameObject !");
                        }
                        break;
                    case SkillType.SkillProjectile:
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                        if (arrow != null)
                        {
                            arrow.Owner = player;
                            arrow.Data = skillData;
                            arrow.PosInfo.State = CreatureState.Moving;
                            arrow.PosInfo.MoveDir = playerInfo.PosInfo.MoveDir;
                            arrow.PosInfo.PosX = playerInfo.PosInfo.PosX;
                            arrow.PosInfo.PosY = playerInfo.PosInfo.PosY;
                            arrow.Speed = skillData.projectile.speed;
                            EnterGame(arrow);
                        }
                        else
                            return;
                        break;
                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                {
                    p.Session.Send(packet);
                }
            }
        }
    }
}
