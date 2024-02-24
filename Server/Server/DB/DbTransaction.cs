using Microsoft.EntityFrameworkCore;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    public class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        //Me(GameRoom) >> You(DB) >> Me(GameRoom)
        public static void SavePlayerStatus_AllInOne(Player player, GameRoom gameRoom)
        {
            if (player == null || gameRoom == null)
                return;

            //Me
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.hp = player.Stat.Hp;
            playerDb.mp = player.Stat.Mp;
            playerDb.level = player.Stat.Level;
            playerDb.totalExp = player.Stat.TotalExp;

            //You
            Instance.Push(() => 
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(playerDb.hp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.mp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.level)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(playerDb.totalExp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        //Me
                        Console.WriteLine($"Hp Saved({playerDb.hp})");
                    }
                }
            });

            
        }

        public static void SavePlayerStatus_Step1(Player player, GameRoom gameRoom)
        {
            if (player == null || gameRoom == null)
                return;

            //Me
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            if(player.Stat.Hp <= 0)
                playerDb.hp = player.Stat.MaxHp;
            else
                playerDb.hp = player.Stat.Hp;
            playerDb.mp = player.Stat.Mp;
            playerDb.level = player.Stat.Level;
            playerDb.totalExp = player.Stat.TotalExp;

            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, gameRoom);
        }

        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom gameRoom)
        {
            using (AppDbContext db = new AppDbContext())
            {
                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(playerDb.hp)).IsModified = true;
                db.Entry(playerDb).Property(nameof(playerDb.mp)).IsModified = true;
                db.Entry(playerDb).Property(nameof(playerDb.level)).IsModified = true;
                db.Entry(playerDb).Property(nameof(playerDb.totalExp)).IsModified = true;
                bool success = db.SaveChangesEx();
                if (success)
                {
                    //Me
                    gameRoom.Push(SavePlayerStatus_Step3, playerDb.hp);
                }
            }
        }

        public static void SavePlayerStatus_Step3(int hp)
        {
            Console.WriteLine($"Hp Saved({hp})");
        }

    }
}

