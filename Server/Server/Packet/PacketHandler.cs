using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

		if(clientSession.MyPlayer == null)
			return;
		if(clientSession.MyPlayer.Room == null)
			return;

		//TODO : 검증

		// 서버에서 좌표이동
		PlayerInfo playerInfo = clientSession.MyPlayer.Info; // 플레이어 정보
		playerInfo.PosInfo = movePacket.PosInfo; // 좌표 이동

		// 방에 있는 모든 플레이어에게 전송
		S_Move resMove = new S_Move();
		resMove.PlayerId = clientSession.MyPlayer.Info.PlayerId;
		resMove.PosInfo = movePacket.PosInfo;
		clientSession.MyPlayer.Room.Broadcast(resMove);

    }
}
