﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
        Managers.Object.Add(enterGamePacket.Player, myPlayer: true);

        //Debug.Log("S_EnterGameHandler");
        //Debug.Log(enterGamePacket.Player);
	}
	
	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
        Managers.Object.Clear();

        //Debug.Log("S_LeaveGameHandler");
    }

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
        S_Spawn spawnPacket = packet as S_Spawn;

        foreach(ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj, myPlayer: false);
        }
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;

        //서버에서 이동패킷이 왔을때 처리해주는 부분
        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;

        if(Managers.Object.MyPlayer.Id == movePacket.ObjectId)  //내 캐릭터의 이동 패킷이면 무시 : 서버에서 패킷을 받아서 처리를 한번더 해서 맞춰준다. >> 부자연스러울수 있음.
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        bc.PosInfo = movePacket.PosInfo;    // 클라이언트 이동을 했지만 서버에서 패킷을 받아서 처리를 한번더 해서 맞춰준다. >> 부자연스러울수 있음.
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;

        //서버에서 이동패킷이 왔을때 처리해주는 부분
        GameObject go = Managers.Object.FindById(skillPacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        {
            cc.UseSkill(skillPacket.Info.SkillId);
        }
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changeHpPacket = packet as S_ChangeHp;

        //서버에서 HP 변경 패킷이 왔을때 처리해주는 부분
        GameObject go = Managers.Object.FindById(changeHpPacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (go != null)
        {
            cc.Hp = changeHpPacket.Hp;
        }
    }

    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;

        //서버에서 HP 변경 패킷이 왔을때 처리해주는 부분
        GameObject go = Managers.Object.FindById(diePacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (go != null)
        {
            cc.Hp = 0;
            cc.OnDead();
        }
    }

    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_ConnectedHandler");
        C_Login loginPacket = new C_Login();
        loginPacket.UniqueId = SystemInfo.deviceUniqueIdentifier;   //디바이스 고유 아이디 알아서 찾아서 넣어주기
        Managers.Network.Send(loginPacket);
    }
    
    // 로그인은 했고, 캐릭터 목록까지 줄게
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;// as S_Login 100% S_Login이므로 강제 캐스팅: 성능이 더 좋음
        Debug.Log($"LoginOk({loginPacket.LoginOk})");

        // TODO: 로비 UI를 보여주고, 캐릭터를 선택해서 게임으로 들어가도록
        if(loginPacket.Players == null || loginPacket.Players.Count == 0)   // 일단은 없으면 만들어서 들어가자
        {
            C_CreatePlayer createPlayerPacket = new C_CreatePlayer();
            createPlayerPacket.Name = $"Player_{Random.Range(0,10000).ToString("0000")}";
            Managers.Network.Send(createPlayerPacket);
        }
        else //있으면 일단 첫번째 캐릭터로 로그인
        { 
            LobbyPlayerInfo info = loginPacket.Players[0];
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = info.Name;
            Managers.Network.Send(enterGamePacket);
        }
    }

    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createPlayerPacket = (S_CreatePlayer)packet;// as S_CreatePlayer 100% S_CreatePlayer이므로 강제 캐스팅: 성능이 더 좋음
        
        if(createPlayerPacket.Player == null)   // 만들기 실패할 경우 null로 오므로
        {
            Debug.Log("CreatePlayer Fail");
            //다시 만들자
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
            Managers.Network.Send(createPacket);
        }
        else // 만들기 성공했으니 일단 첫번째 캐릭터로 로그인
        {
            Debug.Log("CreatePlayer Ok");
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = createPlayerPacket.Player.Name;
            Managers.Network.Send(enterGamePacket);
        }
    }

    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
        S_ItemList itemList = (S_ItemList)packet;// as S_ItemList 100% S_ItemList이므로 강제 캐스팅: 성능이 더 좋음

        //UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        //UI_Inventory InvenUI = gameSceneUI.InvenUI;

        Managers.Inventory.Clear();

        //메모리에 아이템 정보 적용
        foreach(ItemInfo iteminfo in itemList.Items)
        {
            Item item = Item.MakeItem(iteminfo);
            Managers.Inventory.Add(item);
        }

        if(Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();
    }

    public static void S_AddItemHandler(PacketSession session, IMessage packet)
    {
        S_AddItem addItem = (S_AddItem)packet;// as S_AddItem 100% S_AddItem이므로 강제 캐스팅: 성능이 더 좋음


        foreach (ItemInfo iteminfo in addItem.Items)
        {
            Item item = Item.MakeItem(iteminfo);
            //소비아이템인 경우 인벤토리에 없으면 add, 있으면 additemcount
            if (item.ItemType == ItemType.Consumable)   //소비아이템이고
            {
                Item existItem = Managers.Inventory.Get(item.Info.ItemDbId);
                if (existItem != null)  //인벤토리에 있으면
                {
                    Managers.Inventory.AddItemCount(item);
                    continue;
                }
                else    //인벤토리에 없으면
                    Managers.Inventory.Add(item);
            }
            else   //소비아이템이 아니면
                Managers.Inventory.Add(item);
        }

        Debug.Log("아이템을 획득했습니다.");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();

        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();
    }

    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EquipItem equipItem = (S_EquipItem)packet;// as S_AddItem 100% S_AddItem이므로 강제 캐스팅: 성능이 더 좋음

        //메모리에 아이템 정보 적용
        Item item = Managers.Inventory.Get(equipItem.ItemDbId);
        if (item == null)
            return;

        item.Equipped = equipItem.Equipped;
        Debug.Log("아이템 착용 변경");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();

        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();
    }

    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat addItem = (S_ChangeStat)packet;

        //TODO: 스탯 변경 처리
    }
}
