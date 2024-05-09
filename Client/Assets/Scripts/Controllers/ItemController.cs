using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Controllers
{
    public class ItemController: BaseController
    {
        protected ItemType _itemType { get; set; } = ItemType.None;
        protected override void Init()
        {
            base.Init();
        }
    }
}
