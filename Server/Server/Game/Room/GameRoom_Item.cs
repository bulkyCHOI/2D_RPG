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
        
            player.HandleEquipItem(equipPacket);
        }

        public void HandleUseItem(Player player, C_UseItem usePacket)
        {
            if (player == null)
                return;

            player.HandleUseItem(usePacket);
        }
    }
}
