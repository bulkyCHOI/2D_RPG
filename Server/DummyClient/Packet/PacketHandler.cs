using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

class PacketHandler
{
    //Step4. 캐릭터 선택
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changeHpPacket = packet as S_ChangeHp;
    }

    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;
    }

    //Step1. 서버에 연결되었다.
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = new C_Login();
        ServerSession serverSession = (ServerSession)session;
        
        loginPacket.UniqueId = $"DummyClient_{serverSession.DummyId.ToString("0000")}";
        serverSession.Send(loginPacket);

    }

    //Step2. 로그인 성공
    // 로그인은 했고, 캐릭터 목록까지 줄게
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;// as S_Login 100% S_Login이므로 강제 캐스팅: 성능이 더 좋음
        ServerSession serverSession = (ServerSession)session;

        // TODO: 로비 UI를 보여주고, 캐릭터를 선택해서 게임으로 들어가도록
        if (loginPacket.Players == null || loginPacket.Players.Count == 0)   // 일단은 없으면 만들어서 들어가자
        {
            C_CreatePlayer createPlayerPacket = new C_CreatePlayer();
            createPlayerPacket.Name = $"Player_{serverSession.DummyId.ToString("0000")}";
            serverSession.Send(createPlayerPacket);
        }
        else //있으면 일단 첫번째 캐릭터로 로그인
        {
            LobbyPlayerInfo info = loginPacket.Players[0];
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = info.Name;
            enterGamePacket.RoomNumber = 2;
            serverSession.Send(enterGamePacket);
        }
    }

    //Step3. 캐릭터 생성
    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createPlayerPacket = (S_CreatePlayer)packet;// as S_CreatePlayer 100% S_CreatePlayer이므로 강제 캐스팅: 성능이 더 좋음
        ServerSession serverSession = (ServerSession)session;

        if (createPlayerPacket.Player == null)   
        {
            // dummyPlayer이므로 이런 경우는 없다.
        }
        else // 만들기 성공했으니 일단 첫번째 캐릭터로 로그인
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = createPlayerPacket.Player.Name;
            enterGamePacket.RoomNumber = 2;
            serverSession.Send(enterGamePacket);
        }
    }

    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
        S_ItemList itemList = (S_ItemList)packet;// as S_ItemList 100% S_ItemList이므로 강제 캐스팅: 성능이 더 좋음
    }

    public static void S_AddItemHandler(PacketSession session, IMessage packet)
    {
        S_AddItem addItem = (S_AddItem)packet;// as S_AddItem 100% S_AddItem이므로 강제 캐스팅: 성능이 더 좋음
    }

    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EquipItem equipItem = (S_EquipItem)packet;// as S_AddItem 100% S_AddItem이므로 강제 캐스팅: 성능이 더 좋음
    }

    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat addItem = (S_ChangeStat)packet;
    }

    public static void S_UseItemHandler(PacketSession session, IMessage packet)
    {
        S_UseItem useItem = (S_UseItem)packet;
    }

    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        C_Pong pongPacket = new C_Pong();
    }

    public static void S_MoveMapHandler(PacketSession session, IMessage packet)
    {
        S_MoveMap moveMap = (S_MoveMap)packet;
    }

    public static void S_AddExpHandler(PacketSession session, IMessage packet)
    {
        S_AddExp addExp = (S_AddExp)packet;
    }

    public static void S_VendorInteractionHandler(PacketSession session, IMessage packet)
    {
        S_VendorInteraction vendorInteraction = (S_VendorInteraction)packet;
    }

    public static void S_SellItemHandler(PacketSession session, IMessage packet)
    {
        S_SellItem sellItem = (S_SellItem)packet;
    }
}
