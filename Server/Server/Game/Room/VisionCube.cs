using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game.Room
{
    public class VisionCube
    {
        public Player Owner { get; private set; }
        public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();

        public VisionCube(Player owner)
        {
            Owner = owner;
        }

        public HashSet<GameObject> GatherObjects()
        {
            if (Owner.Room == null || Owner.Room == null)
                return null;

            HashSet<GameObject> objects = new HashSet<GameObject>();

            Vector2Int cellPos = Owner.CellPos;
            List<Zone> zones = Owner.Room.GetAdjacentZones(cellPos);

            foreach (Zone zone in zones)
            {
                foreach (Player player in zone.Players)
                {
                    int dx = player.CellPos.x - cellPos.x;
                    int dy = player.CellPos.y - cellPos.y;
                    if(Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                        continue;
                    if (player == Owner)
                        continue;
                    objects.Add(player);
                }
                foreach (Monster monster in zone.Monsters)
                {
                    int dx = monster.CellPos.x - cellPos.x;
                    int dy = monster.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                        continue;
                    objects.Add(monster);
                }
                foreach (Projectile projectile in zone.Projectiles)
                {
                    int dx = projectile.CellPos.x - cellPos.x;
                    int dy = projectile.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                        continue;
                    objects.Add(projectile);
                }
            }

            return objects;
        }

        public void Update()
        {
            if(Owner.Room == null || Owner.Room == null)
                return;

            HashSet<GameObject> currentObjects = GatherObjects();

            //없었는데 새로 생긴 애들 Spawn
            List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();
            if(added.Count > 0)
            {
                S_Spawn spawnPacket = new S_Spawn();
                foreach (GameObject obj in added)
                {
                    ObjectInfo info = new ObjectInfo();
                    info.MergeFrom(obj.Info);
                    spawnPacket.Objects.Add(info);  //참조로 넣지 않는 이유>> 곧바로 전송되는게 아니고 push형태이므로 값복사를 해서 지금 값을 넣어줌
                }
                Owner.Session.Send(spawnPacket);
            }

            //있었는데 사라진 애들 Despawn
            List<GameObject> removed = PreviousObjects.Except(currentObjects).ToList();
            if(removed.Count > 0)
            {
                S_Despawn despawnPacket = new S_Despawn();
                foreach (GameObject obj in removed)
                {
                    despawnPacket.ObjectIds.Add(obj.Id);
                }
                Owner.Session.Send(despawnPacket);
            }

            PreviousObjects = currentObjects;

            Owner.Room.PushAfter(100, Update);
        }
    }
}
