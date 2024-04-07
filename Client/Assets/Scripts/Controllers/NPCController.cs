using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class NPCController : BaseController
{
	protected VendorType _vendorType { get; set; } = VendorType.Normal;
	
	protected override void Init()
	{
		base.Init();
	}
	
}
