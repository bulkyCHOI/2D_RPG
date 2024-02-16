using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.DB;
using Server.Game;
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

		//Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

		//멀티 쓰레드 레드존으므로 한번 꺼내서 null 체크
		Player player = clientSession.MyPlayer;
		if(player == null)
			return;
		GameRoom room = player.Room;
		if(room == null)
			return;

		//oom.HandleMove(player, movePacket);	//이동관련된 것을 room에서 처리하도록 변경
		room.Push(room.HandleMove, player, movePacket);	//Job 방식으로 변경

		//TODO : 검증

		//// 서버에서 좌표이동
		////PlayerInfo playerInfo = clientSession.MyPlayer.Info; // 플레이어 정보 //멀티쓰레드 레드존으로 인해 한번 꺼낸걸로 사용
		//PlayerInfo playerInfo = player.Info; // 플레이어 정보
		//playerInfo.PosInfo = movePacket.PosInfo; // 좌표 이동

		//// 방에 있는 모든 플레이어에게 전송
		//S_Move resMove = new S_Move();
  //      //resMove.PlayerId = clientSession.MyPlayer.Info.PlayerId;	//멀티쓰레드 레드존으로 인해 한번 꺼낸걸로 사용
		//resMove.PlayerId = player.Info.PlayerId;
  //      resMove.PosInfo = movePacket.PosInfo;
  //      //clientSession.MyPlayer.Room.Broadcast(resMove);	//멀티쓰레드 레드존으로 인해 한번 꺼낸걸로 사용
		//room.Broadcast(resMove);
    }

	public static void C_SkillHandler(PacketSession session, IMessage packet)
	{
        C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null)
            return;

        //room.HandleSkill(player, skillPacket);
		room.Push(room.HandleSkill, player, skillPacket); //Job 방식으로 변경
    }

    public static void C_LoginHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = packet as C_Login;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"UniqueId({loginPacket.UniqueId})");

		// TODO: 이런저런 보안 체크

		// DB에서 유저 정보 체크
		// TODO: 문제가 있긴 있다.
		using (AppDbContext db = new AppDbContext())
		{
			AccountDb account = db.Accounts.FirstOrDefault(a => a.AccountName == loginPacket.UniqueId);
			if (account != null)
			{
				S_Login sLogin = new S_Login() { LoginOk = 1 };
				clientSession.Send(sLogin);
            }
			else
			{
                // 계정이 없으면 생성
                AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
                db.Accounts.Add(newAccount);
                db.SaveChanges();

                S_Login sLogin = new S_Login() { LoginOk = 1 };
                clientSession.Send(sLogin);
            }
        }
    }
}
