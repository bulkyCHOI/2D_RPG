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
using SharedDB;

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

		static void StartServerInfoTask()	//간단한 서버 정보를 주기적으로 업데이트 >> flush를 사용하지 않는다.
		{
			var t = new System.Timers.Timer();
			t.AutoReset = true;
			t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
			{
				using (SharedDbContext shared = new SharedDbContext())
				{
					ServerInfoDb serverDb = shared.ServerInfos.Where(s => s.ServerName == Name).FirstOrDefault();
					if (serverDb != null)
					{
						serverDb.ServerIP = IpAddress;
                        serverDb.ServerPort = Port;
						serverDb.BusyCcore = SessionManager.Instance.GetBusyScore();
                        shared.SaveChangesEx();
                    }
                    else
					{
                        shared.ServerInfos.Add(new ServerInfoDb()
						{
                            ServerName = Name,
                            ServerIP = IpAddress,
                            ServerPort = Port,
							BusyCcore = SessionManager.Instance.GetBusyScore()
                        });
                        shared.SaveChangesEx();
					}
				}
            });
			t.Interval = 10 * 1000; // 10초
			t.Start();
		}


		public static string Name { get; } = "Nit Server Public";
		public static int Port { get; } = 80;
		public static string IpAddress { get; set; } 

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();


            GameLogic.Instance.Push(() =>	//아직 main thread에서 실행중이므로 안해도 되지만 push로 실행
            {
				GameRoom room1 = GameLogic.Instance.Add(2, 0);	//1번맵은 만들수 없음 map에 기본적으로 0/1로 이동여부를 판단하고, 2부터는 포탈번호임
                GameRoom room2 = GameLogic.Instance.Add(3, 15);
                GameRoom room3 = GameLogic.Instance.Add(4, 30);
            });

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];
			//string ipAddressString = "175.214.85.227";
			//IPAddress ipAddr = IPAddress.Parse(ipAddressString);
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 80);

			IpAddress = ipAddr.ToString();

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			StartServerInfoTask();

			//DbTask
			{
				Thread t = new Thread(DbTask);
				t.Name = "DB";
				t.Start();
			}

            // NetworkTask
            {
				Thread t = new Thread(NetworkTask);
                t.Name = "Network Send";
                t.Start();
            }

			// GameLogicTask
			Thread.CurrentThread.Name = "GameLogic";
			GameLogicTask();
        }
	}
}
