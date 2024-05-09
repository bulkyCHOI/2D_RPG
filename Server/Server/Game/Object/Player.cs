﻿using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public VisionCube Vision { get; private set; }
        public Inventory Inventory { get; private set; } = new Inventory();

        public int MeleeWeaponDamage { get; private set; }
        public int RangeWeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }

        public override int TotalMeleeAttack { get { return Stat.Attack + MeleeWeaponDamage; } }
        public override int TotalRangeAttack { get { return Stat.Attack + RangeWeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);

            GameRoom newRoom = GameLogic.Instance.Find(2);  //2번방으로 강제 셋팅
            newRoom.EnterGame(this, randPos: true);   //다시 입장   //push로 하지 않아도 된다. 이 함수는 바로 처리된다.

            //MoveScene 패킷을 보내자
            S_MoveMap moveMap = new S_MoveMap();
            moveMap.MapNumber = 2;
            Session.Send(moveMap);
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

        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            Item item = Inventory.GetItem(equipPacket.ItemDbId);
            if (item == null)
                return;

            //if (item.ItemType == ItemType.Consumable)
            //    return;

            //착용 요청이라면, 겹치는 부위 해제
            if (equipPacket.Equipped)
            {
                Item unequipItem = null;
                if (item.ItemType == ItemType.Weapon)
                {
                    WeaponType weaponType = ((Weapon)item).WeaponType;
                    unequipItem = Inventory.Find(
                        i => i.ItemType == ItemType.Weapon && i.Equipped
                        && weaponType == ((Weapon)i).WeaponType);
                }
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unequipItem = Inventory.Find(
                        i => i.ItemType == ItemType.Armor && i.Equipped
                        && armorType == ((Armor)i).ArmorType);
                }
                else if(item.ItemType == ItemType.Consumable)
                {
                    ConsumableType consumableType = ((Consumable)item).ConsumableType;
                    unequipItem = Inventory.Find(
                        i => i.ItemType == ItemType.Consumable && i.Equipped
                        && consumableType == ((Consumable)i).ConsumableType);
                }

                if (unequipItem != null) //아이템 해제
                {
                    //메모리 선 적용
                    unequipItem.Equipped = false;

                    //DB에 적용
                    DbTransaction.EquipItemNoti(this, unequipItem);

                    //클라에게 전송
                    S_EquipItem equipItem = new S_EquipItem();
                    equipItem.ItemDbId = unequipItem.ItemDbId;
                    equipItem.Equipped = unequipItem.Equipped;
                    Session.Send(equipItem);
                }
            }

            //아이템 착용
            {
                //메모리 선 적용
                item.Equipped = equipPacket.Equipped;

                //DB에 적용
                DbTransaction.EquipItemNoti(this, item);

                //클라에게 전송
                S_EquipItem equipItem = new S_EquipItem();
                equipItem.ItemDbId = equipPacket.ItemDbId;
                equipItem.Equipped = equipPacket.Equipped;
                Session.Send(equipItem);
            }

            RefreshAdditionalStat();
        }

        public void RefreshAdditionalStat()
        {
            MeleeWeaponDamage = 0;
            RangeWeaponDamage = 0;
            ArmorDefence = 0;

            foreach (Item item in Inventory.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                switch(item.ItemType)
                {
                    case ItemType.Weapon:
                        if(((Weapon)item).WeaponType == WeaponType.Melee)
                            //소수점 이하 반올림
                            MeleeWeaponDamage += (int)Math.Round(((Weapon)item).Damage * ((((Weapon)item).Enchant * 0.5)+1));
                        else if(((Weapon)item).WeaponType == WeaponType.Range)
                            RangeWeaponDamage += (int)Math.Round(((Weapon)item).Damage * ((((Weapon)item).Enchant * 0.5)+1));
                        break;
                    case ItemType.Armor:
                        ArmorDefence += (int)Math.Round(((Armor)item).Defence * ((((Armor)item).Enchant * 0.5)+1));
                        break;
                }
            }
            //Console.WriteLine($"MeleeWeaponDamage: {MeleeWeaponDamage}");
            //Console.WriteLine($"RangeWeaponDamage: {RangeWeaponDamage}");
            //Console.WriteLine($"ArmorDefence: {ArmorDefence}");
        }

        public void HandleUseItem(C_UseItem usePacket)
        {
            Item item = Inventory.GetItem(usePacket.ItemDbId);
            if (item == null)
                return;

            if (item.ItemType == ItemType.Consumable && item.Count > 0)
            {
                Consumable consumable = (Consumable)item;
                if (consumable.ConsumableType == ConsumableType.HpPortion)
                {
                    //메모리 선 적용 
                    OnHealed(this, consumable.RecoveryAmount);
                    item.Count -= 1;
                    Inventory.EditItemCount(item);
                }
                else if (consumable.ConsumableType == ConsumableType.MpPortion)
                {
                    //메모리 선 적용
                    OnGenMana(this, consumable.RecoveryAmount);
                    item.Count -= 1;
                    Inventory.EditItemCount(item);
                }

                if(item.Count <= 0)
                    Inventory.Items.Remove(usePacket.ItemDbId);

                //TODO UIrefresh

                //DB에 적용
                DbTransaction.UseItemNoti(this, item);

                //클라에게 전송
                S_UseItem useItem = new S_UseItem();
                useItem.ItemDbId = usePacket.ItemDbId;
                Session.Send(useItem);
            }
            RefreshAdditionalStat();
        }

        public void HandleBuyItem(C_BuyItem buyPacket)
        {
            ItemData itemData = DataManager.ItemDict.Values.FirstOrDefault(i => i.id == buyPacket.ItemId);
            if (itemData == null)
                return;

            if (Stat.Gold < itemData.price)
                return;

            Stat.Gold -= itemData.price;
            //골드차감된 내역 플레이어에게 전송
            S_ChangeStat changeStat = new S_ChangeStat();
            changeStat.StatInfo = Stat;
            Session.Send(changeStat);

            //DB에 적용
            RewardData rewardData = new RewardData()
            {
                probability = 100,
                itemId = itemData.id,
                itemCount = 1
            };

            DbTransaction.RewardPlayer(this, rewardData, 0, Room);
            //DbTransaction.BuyItemNoti(this, item);
        }

        public void HandleSellItem(C_SellItem sellPacket)
        {
            Item item = Inventory.GetItem(sellPacket.ItemDbId);
            if (item == null)
                return;

            ItemData itemData = DataManager.ItemDict.Values.FirstOrDefault(i => i.id == item.TemplateId);
            if (itemData == null)
                return;

            Stat.Gold += itemData.price / 2;

            //인벤토리에서 갯수 감소
            item.Count -= 1;
            Inventory.EditItemCount(item);
            if(item.Count <= 0)
                Inventory.Items.Remove(sellPacket.ItemDbId);

            //DB에 적용
            DbTransaction.SellItemNoti(this, item);

            //클라에게 전송
            S_SellItem sellItem = new S_SellItem();
            sellItem.ItemDbId = sellPacket.ItemDbId;
            Session.Send(sellItem);
        }

        public void HandleEnchantItem(C_EnchantItem enchantPacket)
        {
            Item item = Inventory.GetItem(enchantPacket.ItemDbId);
            if (item == null)
                return;

            if (item.ItemType == ItemType.Consumable)
                return;

            int enchantPrice = (item.Grade+1) * item.Price / 2;
            if (Stat.Gold < enchantPrice)
                return;

            Stat.Gold -= enchantPrice;
            //골드차감된 내역 플레이어에게 전송
            S_ChangeStat changeStat = new S_ChangeStat();
            changeStat.StatInfo = Stat;
            Session.Send(changeStat);

            //50% 확률로 강화 성공
            Random random = new Random();
            if (random.Next(0, 2) == 0)
                item.Enchant ++;
            else
                // 실패하면 강화 -1, 0이하로 떨어지지 않게
                item.Enchant = 0;

            //DB에 적용
            DbTransaction.EnchantItemNoti(this, item);

            //클라에게 전송
            S_EnchantItem enchantItem = new S_EnchantItem();
            enchantItem.ItemDbId = enchantPacket.ItemDbId;
            enchantItem.Enchant = item.Enchant;
            Session.Send(enchantItem);
        }

        public void HandleItemSlotChange(C_ItemSlotChange itemSlotChangePacket)
        {
            //slot이 0-30 사이의 값이어야 한다.
            if (itemSlotChangePacket.Slot1 < 0 || itemSlotChangePacket.Slot1 > 30)
                return;
            if (itemSlotChangePacket.Slot2 < 0 || itemSlotChangePacket.Slot2 > 30)
                return;

            Item item1 = Inventory.GetItem(itemSlotChangePacket.Item1DbId);
            Item item2 = Inventory.GetItem(itemSlotChangePacket.Item2DbId);
            if (item1 == null)
                return;
            
            item1.Slot = itemSlotChangePacket.Slot1;
            DbTransaction.ItemSlotChangeNoti(this, item1);

            if (item2 != null)
            {
                item2.Slot = itemSlotChangePacket.Slot2;
                DbTransaction.ItemSlotChangeNoti(this, item2);
            }

            //클라에게 전송
            S_ItemSlotChange itemSlotChange = new S_ItemSlotChange();
            itemSlotChange.Item1DbId = itemSlotChangePacket.Item1DbId;
            itemSlotChange.Item2DbId = itemSlotChangePacket.Item2DbId;
            itemSlotChange.Slot1 = itemSlotChangePacket.Slot1;
            itemSlotChange.Slot2 = itemSlotChangePacket.Slot2;
            Session.Send(itemSlotChange);
        }
    }
}
