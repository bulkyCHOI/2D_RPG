﻿using Google.Protobuf.Protocol;
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
                        //Console.WriteLine($"Hp Saved({playerDb.hp})");
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
                playerDb.hp = player.Stat.MaxHp;    //조건 처리
            else
                playerDb.hp = player.Stat.Hp;       //원 코드는 이렇게 그대로 저장
            playerDb.mp = player.Stat.Mp;
            playerDb.level = player.Stat.Level;
            playerDb.totalExp = player.Stat.TotalExp;
            playerDb.currentExp = player.Stat.CurrentExp;
            playerDb.attack = player.Stat.Attack;
            playerDb.defence = player.Stat.Defence;
            playerDb.speed = player.Stat.Speed;
            playerDb.maxHp = player.Stat.MaxHp;
            playerDb.maxMp = player.Stat.MaxMp;

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
                db.Entry(playerDb).Property(nameof(playerDb.currentExp)).IsModified = true;
                db.Entry(playerDb).Property(nameof(playerDb.attack)).IsModified = true;
                db.Entry(playerDb).Property(nameof(playerDb.defence)).IsModified = true;
                db.Entry(playerDb).Property(nameof(playerDb.speed)).IsModified = true;
                db.Entry(playerDb).Property(nameof(playerDb.maxHp)).IsModified = true;
                db.Entry(playerDb).Property(nameof(playerDb.maxMp)).IsModified = true;
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
            //Console.WriteLine($"Hp Saved({hp})");
        }

        public static void RewardPlayer(Player player, RewardData rewardData, int exp, GameRoom gameRoom)
        {
            if (player == null || gameRoom == null)
                return;

            //TODO : 몬스터에게 획득한 경험치 반영
            player.Stat.CurrentExp += exp;
            //클라이언트에게 획득한 경험치를 알린다.
            {
                S_AddExp expPacket = new S_AddExp();
                expPacket.Exp = exp;
                    
                player.Session.Send(expPacket);
            }
            if (player.Stat.CurrentExp >= player.Stat.TotalExp)
            {
                int level = player.Stat.Level++;
                StatInfo stat = null;
                DataManager.StatDict.TryGetValue(level, out stat);
                if (stat != null)
                {
                    player.Stat.MaxHp = stat.MaxHp;
                    player.Stat.Hp = player.Stat.MaxHp;
                    player.Stat.MaxMp = stat.MaxMp;
                    player.Stat.Mp = player.Stat.MaxMp;
                    player.Stat.Attack = stat.Attack;
                    player.Stat.Defence = stat.Defence;
                    player.Stat.TotalExp = stat.TotalExp;
                    player.Stat.CurrentExp = 0;
                }

                //클라이언트에게 스탯이 변경됨을 알린다.
                {
                    S_ChangeStat changeStatPacket = new S_ChangeStat();
                    changeStatPacket.StatInfo = new StatInfo(player.Stat);

                    player.Session.Send(changeStatPacket);
                }

                //DB에 저장
                SavePlayerStatus_Step1(player, gameRoom);
            }
            Console.WriteLine($"Exp: {exp}: {player.Stat.CurrentExp}/{player.Stat.TotalExp}");

            if (rewardData == null) //획득 아이템이 없다면 패스
                return;
            Item consumableItem = player.Inventory.Find(
                        i => i.TemplateId == rewardData.itemId  //소지한
                        && i.ItemType == ItemType.Consumable    //소비아이템인 경우
                        ); //소비아이템 겹치기 처리하기 위해
            if (consumableItem != null) //소지한 소비아이템인 경우
            {
                //Console.WriteLine("소비아이템 획득");
                Instance.Push(() =>
                {
                    using (AppDbContext db = new AppDbContext())
                    {
                        ItemDb updateItemDB = db.Items.FirstOrDefault(i => i.ItemDbId == consumableItem.Info.ItemDbId);

                        if (updateItemDB != null)
                        {
                            updateItemDB.Count += rewardData.itemCount;

                            bool success = db.SaveChangesEx();
                            if (success)
                            {
                                //Me
                                gameRoom.Push(() =>
                                {
                                    //Item newItem = Item.MakeItem(itemDb);
                                    //player.Inventory.AddItem(newItem);
                                    Item updateItem = Item.MakeItem(updateItemDB);
                                    player.Inventory.EditItemCount(updateItem);

                                    //클라이언트에게 획득한 아이템을 알린다.
                                    {
                                        S_AddItem itemPacket = new S_AddItem();
                                        ItemInfo itemInfo = new ItemInfo();
                                        itemInfo.MergeFrom(updateItem.Info);
                                        //itemInfo.Count += 1; //이미 더했다.
                                        itemPacket.Items.Add(itemInfo);

                                        player.Session.Send(itemPacket);
                                    }
                                });
                            }
                        }
                    }
                });
            }
            else //일반 장비일경우 or 소지하지 않은 소비아이템    
            {
                // 1) DB에 저장 요청
                // 2) DB에 저장 요청이 완료되면
                // 3) 메모리에 적용한다.
                // 트랜젝션 형태이기 때문에 거의 동시에 GetEmptySlot() 호출이 가능하게 되는데
                // 이때 같은 슬롯에 여러 아이템을 넣게 되는 경우가 생길 수 있다.
                int? slot = player.Inventory.GetEmptySlot();
                if (slot == null) //빈칸이 없으면 획득안함.
                    return;

                //Me
                ItemDb itemDb = new ItemDb()
                {
                    TemplateId = rewardData.itemId,
                    Count = rewardData.itemCount,
                    Slot = slot.Value,  //
                    OwnerDbId = player.PlayerDbId
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
                            //Me
                            gameRoom.Push(() =>
                            {
                                Item newItem = Item.MakeItem(itemDb);
                                player.Inventory.AddItem(newItem);

                                //클라이언트에게 획득한 아이템을 알린다.
                                {
                                    S_AddItem itemPacket = new S_AddItem();
                                    ItemInfo itemInfo = new ItemInfo();
                                    itemInfo.MergeFrom(newItem.Info);
                                    itemPacket.Items.Add(itemInfo);

                                    player.Session.Send(itemPacket);
                                }
                            });
                        }
                    }
                });
            }
        }
    }
}

