using Google.Protobuf;
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
    
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;// as S_Login 100% S_Login이므로 강제 캐스팅: 성능이 더 좋음
        Debug.Log($"LoginOk({loginPacket.LoginOk})");
    }
}
