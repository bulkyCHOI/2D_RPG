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
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null)
                return;
        
            Item item = player.Inventory.GetItem(equipPacket.ItemDbId);
            if (item == null)
                return;

            //메모리 선 적용
            item.Equipped = equipPacket.Equipped;

            //DB에 적용
            DbTransaction.EquipItemNoti(player, item);

            //클라에게 전송
            S_EquipItem equipItem = new S_EquipItem();
            equipItem.ItemDbId = equipPacket.ItemDbId;
            equipItem.Equipped = equipPacket.Equipped;
            player.Session.Send(equipItem);
        }
    }
}
