using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    #region Stat
    [Serializable]
    public class StatData : ILoader<int, StatInfo>
    {
        public List<StatInfo> stats = new List<StatInfo>();

        public Dictionary<int, StatInfo> MakeDict()
        {
            Dictionary<int, StatInfo> dict = new Dictionary<int, StatInfo>();
            foreach (StatInfo stat in stats)
            {
                stat.Hp = stat.MaxHp;
                stat.Mp = stat.MaxMp;
                dict.Add(stat.Level, stat);
            }
            return dict;
        }
    }
    #endregion

    #region Skill
    [Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public float cooldown;
        public int damage;
        public SkillType skillType;
        public ProjectileInfo projectile;
    }

    public class ProjectileInfo
    {
        public string name;
        public float speed;
        public int range;
        public string prefab;
    }

    [Serializable]
    public class SkillData : ILoader<int, Skill>
    {
        public List<Skill> skills = new List<Skill>();

        public Dictionary<int, Skill> MakeDict()
        {
            Dictionary<int, Skill> dict = new Dictionary<int, Skill>();
            foreach (Skill skill in skills)
                dict.Add(skill.id, skill);
            return dict;
        }
    }
    #endregion

    #region Item
    [Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public ItemType itemType;
    }

    public class WeaponData : ItemData
    {
        public WeaponType weaponType;
        public int damage;
    }

    public class ArmorData : ItemData
    {
        public ArmorType armorType;
        public int defence;
    }

    public class ConsumableData : ItemData
    {
        public ConsumableType consumableType;
        public int recoveryAmount;
    }

    [Serializable]
    public class ItemLoader : ILoader<int, ItemData>
    {
        public List<WeaponData> weapons = new List<WeaponData>();
        public List<ArmorData> armors = new List<ArmorData>();
        public List<ConsumableData> consumables = new List<ConsumableData>();

        public Dictionary<int, ItemData> MakeDict()
        {
            Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
            foreach (ItemData item in weapons)
            {
                item.itemType = ItemType.Weapon;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in armors)
            {
                item.itemType = ItemType.Armor;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in consumables)
            {
                item.itemType = ItemType.Consumable;
                dict.Add(item.id, item);
            }
            return dict;   
        }
    }
    #endregion

    #region Monster
    [Serializable]
    public class RewardData
    {
        public int probability; //100분율 1~100%
        public int itemId;
        public int itemCount;
    }

    [Serializable]
    public class MonsterData
    {
        public int id;
        public string name;
        public StatInfo stat;   //stat 안에 totalExp가 있음 >> 획득 exp로 사용하자
        public List<RewardData> rewards;
    }

    [Serializable]
    public class MonsterLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();

        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monster in monsters)
            {
                dict.Add(monster.id, monster);
            }
            return dict;
        }
    }
    #endregion

    #region Vendor
    [Serializable]
    public class VendorItemData
    {
        public int itemId;
        public int slot;
        public int price;
    }

    [Serializable]
    public class VendorData
    {
        public int id;
        public string name;
        public VendorType vendorType;
        public List<VendorItemData> items;
    }

    [Serializable]
    public class VendorLoader : ILoader<int, VendorData>
    {
        public List<VendorData> vendors = new List<VendorData>();

        public Dictionary<int, VendorData> MakeDict()
        {
            Dictionary<int, VendorData> dict = new Dictionary<int, VendorData>();
            foreach (VendorData vendor in vendors)
            {
                dict.Add(vendor.id, vendor);
            }
            return dict;
        }
    }
    #endregion
}
