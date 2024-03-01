using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;

namespace Server
{
	// 1. GameRoom 방식의 간당한 동기화 >> 완료
	// 2. 더 넓은 영역 관리
	// 3. 심리스 MMO

	// 현재 쓰레드 상황
	// 1. Recv(N개)				서빙(주문받는)
	// 2. GameLogic(1개)		요리사
	// 3. NetworkSend(1개)		서빙(요리완료)
	// 4. DB(1개)				결제/장부

    class Program
	{
		static Listener _listener = new Listener();

		static void GameLogicTask()
		{
            while (true)
            {
                GameLogic.Instance.Update();
                Thread.Sleep(0);
            }
        }

		static void DbTask()
		{
            while (true)
			{
                DbTransaction.Instance.Flush();
                Thread.Sleep(0);
            }
        }

		static void NetworkTask()
		{
            while (true)
			{
				List<ClientSession> sessions = SessionManager.Instance.GetSessions();
				foreach (ClientSession s in sessions)
				{
					s.FlushSend();
				}
                Thread.Sleep(0);
            }
        }

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();


            GameLogic.Instance.Push(() =>	//아직 main thread에서 실행중이므로 안해도 되지만 push로 실행
            {
				GameRoom room = GameLogic.Instance.Add(1);
            });

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			// GameLogicTask
			{
				Task gameLogicTask = new Task(GameLogicTask, TaskCreationOptions.LongRunning);	// TaskCreationOptions.LongRunning : 즉시 실행 가능한 Task를 생성
				gameLogicTask.Start();
			}

            // NetworkTask
            {
				Task networkTask = new Task(NetworkTask, TaskCreationOptions.LongRunning);
                networkTask.Start();
            }

			//DbTask
			DbTask();	//마지막 이어야 한다.

        }
	}
}
