using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Inventory  //Invertory 클래스는 Item 클래스를 관리하는 클래스
    {
        public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

        public void AddItem(Item item)
        {
            Items.Add(item.ItemDbId, item);
        }

        public Item GetItem(int id)
        {
            Item item = null;
            Items.TryGetValue(id, out item);
            return item;
        }

        public Item Find(Func<Item, bool> condition)    //Func<Item, bool> 델리게이트를 사용하여 조건을 받아들이는 Find 메서드
        {
            foreach (Item item in Items.Values)    //foreach문을 사용하여 _items의 모든 요소를 순회
            {
                if (condition.Invoke(item)) //condition 델리게이트를 호출하여 조건을 검사
                    return item;
            }
            return null;
        }

        public int? GetEmptySlot()  //GetEmptySlot 메서드는 빈 슬롯을 찾아서 반환
        {
            for (int slot = 0; slot < 30; slot++)    //16개의 슬롯을 순회하면서 빈 슬롯을 찾음
            {
                Item item = Items.Values.FirstOrDefault(i => i.Slot == slot);
                if (item == null)
                    return slot;
            }
            return null;
        }
    }
}
