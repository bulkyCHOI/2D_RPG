using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public int Id 
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; } 
        }

        public string Name { get; set;}

        public GameRoom Room { get; set; }

        public ObjectInfo Info { get; set; } = new ObjectInfo();
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();
        public StatInfo Stat { get; private set; } = new StatInfo();

        public virtual int TotalMeleeAttack { get { return Stat.Attack; }}
        public virtual int TotalRangeAttack { get { return Stat.Attack; }}
        public virtual int TotalDefence { get { return 0; }}

        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }

        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); }
        }

        public MoveDir Dir
        {
            get { return PosInfo.MoveDir; }
            set { PosInfo.MoveDir = value; }
        }

        public CreatureState State
        {
            get { return PosInfo.State; }
            set { PosInfo.State = value; }
        }
        
        public GameObject()
        {
            Info.PosInfo = PosInfo;
            Info.StatInfo = Stat;
        }

        public virtual void Update()
        {
        }

        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            }
            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
            }
        }

        public Vector2Int GetFrontCellPos()
        {
            return GetFrontCellPos(PosInfo.MoveDir);
        }

        public Vector2Int GetFrontCellPos(MoveDir dir) //앞에 있는 셀의 위치를 반환
        {
            Vector2Int cellPos = CellPos;

            switch (dir)
            {
                case MoveDir.Up:
                    cellPos += Vector2Int.up;
                    break;
                case MoveDir.Down:
                    cellPos += Vector2Int.down;
                    break;
                case MoveDir.Left:
                    cellPos += Vector2Int.left;
                    break;
                case MoveDir.Right:
                    cellPos += Vector2Int.right;
                    break;
            }

            return cellPos;
        }

        public static MoveDir GetDirFromVec(Vector2Int dir)
        {
            if (dir.x > 0)
                return MoveDir.Right;
            else if (dir.x < 0)
                return MoveDir.Left;
            else if (dir.y > 0)
                return MoveDir.Up;
            else
                return MoveDir.Down;
        }

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            if (Room == null)
                return;
            if (ObjectType == GameObjectType.Npc)
                return;

            damage = Math.Max(0, damage - TotalDefence);
            Stat.Hp = Math.Max(0, Stat.Hp - damage);

            S_ChangeHp changeHpPacket = new S_ChangeHp();
            changeHpPacket.ObjectId = Id;
            changeHpPacket.Hp = Stat.Hp;
            Room.Broadcast(CellPos, changeHpPacket);

            if(Stat.Hp <= 0)
            {
                //죽음
                OnDead(attacker);
            }
        }

        public virtual void OnHealed(GameObject attacker, int heal)
        {
            if (Room == null)
                return;

            Stat.Hp = Math.Min(Stat.MaxHp, Stat.Hp + heal);

            S_ChangeHp changeHpPacket = new S_ChangeHp();
            changeHpPacket.ObjectId = Id;
            changeHpPacket.Hp = Stat.Hp;
            Room.Broadcast(CellPos, changeHpPacket);
        }

        public virtual void OnGenMana(GameObject attacker, int recoveryMana)
        {
            if (Room == null)
                return;

            Stat.Mp = Math.Min(Stat.MaxMp, Stat.Mp + recoveryMana);

            S_ChangeMp changeMpPacket = new S_ChangeMp();
            changeMpPacket.ObjectId = Id;
            changeMpPacket.Mp = Stat.Mp;
            Room.Broadcast(CellPos, changeMpPacket);
        }

        public virtual void OnDead(GameObject attacker)
        {
            if(Room == null)
                return;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(CellPos, diePacket);

            GameRoom room = Room;   //Room에서 나가기 전에 Room을 저장해놓는다.
            room.LeaveGame(Id); //push로 하지 않아도 된다. 이 함수는 바로 처리된다.
            //room.Push(room.LeaveGame, Id); //Job 방식으로 변경

            //이부분은 Job으로 인해 나중에 처리될수 있어 문제가 발생될수 있다.
            //플레이어가 나가지 않은 상태에서 위치나 방향들을 아래처럼 초기화 되면
            //Map에서는 0,0만 null로 밀어버리므로, 마지막 좌표에는 계속 플레이어가 있는 것처럼 된다.
            Stat.Hp = Stat.MaxHp;
            PosInfo.State = CreatureState.Idle;
            PosInfo.MoveDir = MoveDir.Down;

            if (ObjectType == GameObjectType.Player)    //플레이어는 2번방인 마을로
            {
                GameRoom newRoom = GameLogic.Instance.Find(2);  //2번방으로 강제 셋팅
                newRoom.EnterGame(this, randPos:true);   //다시 입장   //push로 하지 않아도 된다. 이 함수는 바로 처리된다.
            }
            else    //플레이어 외는 그냥 지금 맵으로 재입장
            {
                room.EnterGame(this, randPos:true);   //다시 입장   //push로 하지 않아도 된다. 이 함수는 바로 처리된다.
            }
            //room.Push(room.EnterGame, this);   //다시 입장  //Job 방식으로 push

            //매우중요!!!!
            //push로 하지 않아도 되는 이유는 room이 JobSerializer를 상속받았기 때문이다.
            //JobSerializer를 상속받은 클래스는 Job으로 처리되는 함수를 호출할때, 자동으로 push를 해주기 때문이다.
            //따라서, Job으로 처리되는 함수를 호출할때, push를 하지 않아도 된다.
            //push를 하면 발생되는 위의 문제들 때문에 push를 하지 않고 처리한다.
        }

        public virtual GameObject GetOwner()    //보상을 줄때 사용하기 위함: 펫이나 투사체가 막타를 날린 경우, 플레이어에게 경험치를 줘야한다.
        {
            return this;
        }
    }
}
