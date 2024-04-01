using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
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

        public int WeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
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
                        i => i.ItemType == ItemType.Weapon && i.Equipped);
                        //&& weaponType == ((Weapon)i).WeaponType);
                }
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unequipItem = Inventory.Find(
                        i => i.ItemType == ItemType.Armor && i.Equipped
                        && armorType == ((Armor)i).ArmorType);
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
            WeaponDamage = 0;
            ArmorDefence = 0;

            foreach (Item item in Inventory.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                switch(item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;
                    case ItemType.Armor:
                        ArmorDefence += ((Armor)item).Defence;
                        break;
                }
            }
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
    }
}
