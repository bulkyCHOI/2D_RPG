using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleVendorInteraction(Player player)
        {
            //Console.WriteLine("Interaction start");
            if (player == null)
                return;

            ObjectInfo playerInfo = player.Info; // 플레이어 정보
            if (playerInfo.PosInfo.State != CreatureState.Idle)  //이동중이면 무효처리
                return;

            //캐릭터 앞에 어떤 gameobject가 있는지 확인
            Vector2Int vendorPos = player.GetFrontCellPos(playerInfo.PosInfo.MoveDir);
            GameObject target = Map.Find(vendorPos);
            if (target == null) return;
            if (target.ObjectType != GameObjectType.Npc) return;
            NPC npc = (NPC)target;
            //Console.WriteLine($"Interaction with: {npc.VendorType}"); 

            //패킷을 보내자
            S_VendorInteraction vInteraction = new S_VendorInteraction();
            vInteraction.VendorType = npc.VendorType;
            //아이템리스트를 보내야 한다.
            if (npc.VendorData != null && npc.VendorData.items != null)
            {
                foreach (var item in npc.VendorData.items)
                {
                    ItemData itemInfo = null;
                    DataManager.ItemDict.TryGetValue(item.itemId, out itemInfo);
                    if (itemInfo == null)
                        continue;

                    VendorItemInfo itemData = new VendorItemInfo();
                    itemData.ItemId = item.itemId;
                    itemData.Slot = item.slot;
                    itemData.Price = item.price;
                    vInteraction.Items.Add(itemData);
                }
            }   
            player.Session.Send(vInteraction);
        }
    }
}
