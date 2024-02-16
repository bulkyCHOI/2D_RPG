using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Game;
using Server.Data;
using System.Security.AccessControl;

namespace Server
{
	public class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; }	// 플레이어 정보
		public int SessionId { get; set; }	

		public void Send(IMessage packet)	// 프로토콜을 받아서 보내는 함수
		{
			string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            Send(new ArraySegment<byte>(sendBuffer));
        }

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			{ 
				S_Connected sConnPacket = new S_Connected();
				Send(sConnPacket);
			}

			// TODO: 로비에서 캐릭터 선택하도록 만들기
			MyPlayer = ObjectManager.Instance.Add<Player>();	// 플레이어 생성
			{ 
				MyPlayer.Info.Name = $"Player_{MyPlayer.Info.ObjectId}";
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;

				StatInfo stat = null;
				DataManager.StatDict.TryGetValue(1, out stat);
				MyPlayer.Stat.MergeFrom(stat);

                MyPlayer.Session = this;
			}

			// TODO : 방에 입장하는 로직을 구현해야 함
			//RoomManager.Instance.Find(1).EnterGame(MyPlayer);	// 방에 플레이어 입장
			GameRoom room = RoomManager.Instance.Find(1);
			room.Push(room.EnterGame, MyPlayer);	// 방에 플레이어 입장	//Job 방식으로 변경
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			//RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.ObjectId);	// 방에서 플레이어 퇴장
			GameRoom room = RoomManager.Instance.Find(1);
			room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);	// 방에서 플레이어 퇴장	//Job 방식으로 변경
            SessionManager.Instance.Remove(this);
			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
