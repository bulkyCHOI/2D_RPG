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
        public VendorData VendorData { get; set; } = null;
        

        public NPC()
        {
            ObjectType = GameObjectType.Npc;
        }
        public void Init(int templateId)
        {
            TemplateId = templateId;
            VendorData vendorData = null;
            DataManager.VendorDict.TryGetValue(templateId, out vendorData);
            Name = vendorData.name;
            Info.Name = vendorData.name;
            VendorType = vendorData.vendorType;
            VendorData = vendorData;
            //아이템
            //vendorData.itemList
                        
        }
    }
}
