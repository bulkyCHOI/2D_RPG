using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public Invertory Invertory { get; private set; } = new Invertory();

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }

        public void OnLeaveGame()
        {
            //DB연동?
            //게임에서 나갈때만 연동한다.
            //1) 서버가 다운되면? 저장되지 않은 데이터는 날아감.
            //2) 코드 흐름을 다 막아버린다.
            //OnLeaveGame을 호출하는 것은 GameRoom 뿐이다.
            //그런데 GameRoom에서는 JobSerializer를 상속받았기 때문에 >> LOCK!!!
            //OnLeaveGame을 호출하고 나서 다른 일을 처리할 수 있다.
            //그래서 DB연동을 하는 무거운 일을 처리하므로 다른 코드의 흐름을 다 잡아먹는다.
            //해결방법
            //비동기   
            //다른 쓰레드에서 처리한다.
            //그런데 결과를 받고 그 다음 이어서 일을 처리해야하는 경우가 많음
            //예를 들면 아이템 획득하고 그 아이템을 강화하는 경우
            //DB에 아이템이 없는데도, 아이템을 강화한다??
            //>>> 쓰레드로 던저서 처리하고 결과를 받아 이후에 처리할 일을 진행해야 한다.
            //>>>> job 방식
            //using (AppDbContext db = new AppDbContext())
            //{
            //    //PlayerDb playerDb = db.Players.Find(PlayerDbId);    //PlayerDbId로 찾아서 1번, 저장에 1번 총 2번의 DB hit
            //    PlayerDb playerDb = new PlayerDb();
            //    playerDb.PlayerDbId = PlayerDbId;
            //    playerDb.hp = Stat.Hp;
            //    playerDb.mp = Stat.Mp;
            //    playerDb.level = Stat.Level;
            //    playerDb.totalExp = Stat.TotalExp;

            //    //이렇게 하면 1번의 DB hit으로 끝난다.
            //    db.Entry(playerDb).State = EntityState.Unchanged;
            //    db.Entry(playerDb).Property(nameof(playerDb.hp)).IsModified = true;
            //    db.Entry(playerDb).Property(nameof(playerDb.mp)).IsModified = true;
            //    db.Entry(playerDb).Property(nameof(playerDb.level)).IsModified = true;
            //    db.Entry(playerDb).Property(nameof(playerDb.totalExp)).IsModified = true;
            //    db.SaveChangesEx();

            //    Console.WriteLine($"Hp Saved({playerDb.hp})");
            //}

            //DbTransaction.SavePlayerStatus_AllInOne(this, Room);
            DbTransaction.SavePlayerStatus_Step1(this, Room);
        }
    }
}
