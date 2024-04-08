using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class NPC : GameObject
    {
        public int TemplateId { get; private set; }
        public VendorType VendorType { get; set; } = VendorType.Normal;

        public NPC()
        {
            ObjectType = GameObjectType.Npc;
        }
        public void Init(int templateId)
        {
            TemplateId = templateId;

            //MonsterData monsterData = null;
            //DataManager.MonsterDict.TryGetValue(templateId, out monsterData);
            //Stat.MergeFrom(monsterData.stat);
            //Stat.Hp = Stat.MaxHp;
            //State = CreatureState.Idle;
        }
    }
}
