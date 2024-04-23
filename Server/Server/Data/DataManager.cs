using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        public static Dictionary<int, StatInfo> StatDict { get; private set; } = new Dictionary<int, StatInfo>();
        public static Dictionary<int, Data.Skill> SkillDict { get; private set; } = new Dictionary<int, Data.Skill>();
        public static Dictionary<int, Data.ItemData> ItemDict { get; private set; } = new Dictionary<int, Data.ItemData>();
        public static Dictionary<int, Data.MonsterData> MonsterDict { get; private set; } = new Dictionary<int, Data.MonsterData>();
        public static Dictionary<int, Data.VendorData> VendorDict { get; private set; } = new Dictionary<int, Data.VendorData>();


        public static void LoadData()
        {
            StatDict = LoadJson<Data.StatData, int, StatInfo>("StatData").MakeDict();
            SkillDict = LoadJson<Data.SkillData, int, Data.Skill>("SkillData").MakeDict();
            ItemDict = LoadJson<Data.ItemLoader, int, Data.ItemData>("ItemData").MakeDict();
            MonsterDict = LoadJson<Data.MonsterLoader, int, Data.MonsterData>("MonsterData").MakeDict();
            VendorDict = LoadJson<Data.VendorLoader, int, Data.VendorData>("VendorData").MakeDict();
        }

        public static void UpdateVendorData()
        {
            //price를 ItemData에서 가져와서 VendorData에 업데이트
            foreach (var kvp in VendorDict)
            {
                VendorData vendorData = kvp.Value;
                foreach (var item in vendorData.items)
                {
                    ItemData itemData = null;
                    if (ItemDict.TryGetValue(item.itemId, out itemData))
                    {
                        item.price = itemData.price;
                    }
                }
            }
        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            //유니티쪽 코드이므로 주석처리
            //TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
            //return JsonUtility.FromJson<Loader>(textAsset.text);
            //동일하게 C# only 코드로 변경
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
    }

}
