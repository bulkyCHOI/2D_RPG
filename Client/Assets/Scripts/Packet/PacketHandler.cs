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
        Managers.Object.RemoveMyPlayer();

        //Debug.Log("S_LeaveGameHandler");
    }

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
        S_Spawn spawnPacket = packet as S_Spawn;

        foreach(PlayerInfo player in spawnPacket.Players)
        {
            Managers.Object.Add(player, myPlayer: false);
        }
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int id in despawnPacket.PlayerIds)
        {
            Managers.Object.Remove(id);
        }
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;
        ServerSession serverSession = session as ServerSession;

        //서버에서 이동패킷이 왔을때 처리해주는 부분
        GameObject go = Managers.Object.FindById(movePacket.PlayerId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
            return;

        cc.PosInfo = movePacket.PosInfo;    // 클라이언트 이동을 했지만 서버에서 패킷을 받아서 처리를 한번더 해서 맞춰준다. >> 부자연스러울수 있음.
    }
}
