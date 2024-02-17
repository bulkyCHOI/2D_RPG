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
        long _nextMoveTick = 0;

        public GameObject Owner { get; set; }

        public override void Update()
        {
            if(Data == null || Data.projectile == null ||Owner == null || Room == null)
                return;
            if(Environment.TickCount <= _nextMoveTick)
                return;

            long tick = (long)(1000 / Data.projectile.speed);
            _nextMoveTick = Environment.TickCount + tick;   //기다려야 하는 시간

            Vector2Int destPos = GetFrontCellPos();
            if(Room.Map.CanGo(destPos)) // 이동 가능한가? 날아가기
            {
                CellPos = destPos;

                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(movePacket);

                Console.WriteLine("Move Arrow");
            }
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if (target != null)
                {
                    // 피격 판정
                    target.OnDamaged(this, Data.damage + Owner.Stat.DEX);
                }
                //소멸
                //Room.LeaveGame(Id); 
                Room.Push(Room.LeaveGame, Id); //Job 방식으로 변경
            }
        }
    }
}
