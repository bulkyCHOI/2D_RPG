using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class DropItem: GameObject
    {
        RewardData _rewardData;
        public RewardData RewardData { get { return _rewardData; } set { _rewardData = value; } }
        public DropItem()
        {
            ObjectType = GameObjectType.Item;
        }

    }
}
