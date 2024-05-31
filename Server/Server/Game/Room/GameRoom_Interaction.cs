using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Migrations;
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

            Vector2Int itemPos = player.CellPos;
            GameObject targetItem = Map.FindItem(itemPos);
            
            if (target == null && targetItem == null) return;

            if (target != null)
            {
                if (target.ObjectType == GameObjectType.Npc)
                {
                    // NPC와 상호작용
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

            if (targetItem != null)
            {
                if (targetItem.ObjectType == GameObjectType.Item)
                {
                    // 아이템 줍기
                    DropItem dropItem = (DropItem)targetItem;
                    Console.WriteLine($"Interaction with: {dropItem.RewardData.itemId}");
                    DbTransaction.RewardPlayer(player, dropItem.RewardData, 0, this);   //경험치는 0으로 처리
                    Push(LeaveGame, targetItem.Id); 
                }
            }
        }
    }
}
