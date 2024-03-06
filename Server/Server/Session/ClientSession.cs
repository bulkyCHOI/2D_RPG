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
	public partial class ClientSession : PacketSession	//partial 키워드로 나눠진 클래스를 합침:c#전용
	{
		public PlayerServerState ServerState { get; set; } = PlayerServerState.ServerStateLogin;	// 플레이어 상태
		public Player MyPlayer { get; set; }	// 플레이어 정보
		public int SessionId { get; set; }

		object _lock = new object();	// lock을 위한 오브젝트
		List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();	// 패킷을 보내기 위한 큐

		long _pingpongTick = 0;	// 핑퐁 시간
		public void Ping()
		{
			if(_pingpongTick != 0)
			{
                if(System.Environment.TickCount64 - _pingpongTick >= 30 *1000) // 30초가 지나도 핑퐁이 없으면
				{
                    Console.WriteLine("Disconneted by Ping-Pong Check");
                    Disconnect();
                    return;
                }
            }

			S_Ping pingPacket = new S_Ping();
			Send(pingPacket);

			GameLogic.Instance.PushAfter(5000, Ping);	// 5초마다 핑퐁
		}

		public void HandlePong()
		{
			_pingpongTick = System.Environment.TickCount64;
		}

        #region Network
		//큐에 예약만 해두고
        public void Send(IMessage packet)	// 프로토콜을 받아서 보내는 함수
		{
			string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

			lock (_lock)
			{
				_reserveQueue.Add(sendBuffer);
            }
            //Send(new ArraySegment<byte>(sendBuffer));
        }

		//큐에 예약된 패킷을 보냄
		public void FlushSend()
		{
            List<ArraySegment<byte>> sendList = null;

            lock (_lock)
			{
                if (_reserveQueue.Count == 0)
                    return;

                sendList = _reserveQueue;
                _reserveQueue = new List<ArraySegment<byte>>();
            }

            Send(sendList);
        }
        
		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			{ 
				S_Connected sConnPacket = new S_Connected();
				Send(sConnPacket);
			}

			GameLogic.Instance.PushAfter(5000, Ping);	
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
            //RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.ObjectId);	// 방에서 플레이어 퇴장
            GameLogic.Instance.Push(() =>
            {
				if(MyPlayer == null)
					return;

                GameRoom room = GameLogic.Instance.Find(1);
				room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);	// 방에서 플레이어 퇴장	//Job 방식으로 변경
            });
            SessionManager.Instance.Remove(this);
			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
        #endregion
    }
}
