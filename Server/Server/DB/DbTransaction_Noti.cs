using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            //Me
            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped,
            };

            //You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(itemDb.Equipped)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        //Me는 미처리
                        //실패하면 Kick
                    }
                }
            });
        }

        public static void UseItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            //You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ItemDb updateItemDB = db.Items.FirstOrDefault(i => i.ItemDbId == item.Info.ItemDbId);

                    updateItemDB.Count --;
                    if(updateItemDB.Count <= 0)
                        db.Items.Remove(updateItemDB);

                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        //Me는 미처리
                        //실패하면 Kick
                    }
                }
            });
        }

        public static void BuyItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            //Me
            ItemDb itemDb = new ItemDb()
            {
                TemplateId = item.TemplateId,
                Count = item.Count,
            };

            //You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Items.Add(itemDb);

                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        //Me는 미처리
                        //실패하면 Kick
                    }
                }
            });
        }

        public static void SellItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            //You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ItemDb updateItemDB = db.Items.FirstOrDefault(i => i.ItemDbId == item.Info.ItemDbId);

                    updateItemDB.Count --;
                    if(updateItemDB.Count <= 0)
                        db.Items.Remove(updateItemDB);



                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        //Me는 미처리
                        //실패하면 Kick
                    }
                }
            });
        }
    }
}

