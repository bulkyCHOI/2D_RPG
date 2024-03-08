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

    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = new C_Login();
    }

    // 로그인은 했고, 캐릭터 목록까지 줄게
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;// as S_Login 100% S_Login이므로 강제 캐스팅: 성능이 더 좋음
    }

    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createPlayerPacket = (S_CreatePlayer)packet;// as S_CreatePlayer 100% S_CreatePlayer이므로 강제 캐스팅: 성능이 더 좋음
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
}
