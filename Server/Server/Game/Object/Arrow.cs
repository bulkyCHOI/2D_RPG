using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Arrow : Projectile
    {
        public GameObject Owner { get; set; }

        public override void Update()
        {
            if(Data == null || Data.projectile == null ||Owner == null || Room == null)
                return;
        
            int tick = (int)(1000 / Data.projectile.speed);
            Room.PushAfter(tick, Update);

            Vector2Int destPos = GetFrontCellPos();
            if(Room.Map.ApplyMove(this, destPos, collision: false)) // 이동 가능한가? 날아가기
            {
                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(CellPos, movePacket);

                //Console.WriteLine("Move Arrow");
            }
            else    // 누가 맞았는가?
            {
                GameObject target = Room.Map.Find(destPos);
                if (target != null)
                {
                    // 피격 판정
                    target.OnDamaged(this, Data.damage + Owner.TotalRangeAttack);
                }
                //소멸
                //Room.LeaveGame(Id); 
                Room.Push(Room.LeaveGame, Id); //Job 방식으로 변경
            }
        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
