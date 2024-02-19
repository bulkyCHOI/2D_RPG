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
    class Program
	{
		static Listener _listener = new Listener();
		static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

		static void TickRoom(GameRoom room, int tick = 100)	//100ms마다 업데이트
		{
			var timer = new System.Timers.Timer();
			timer.Interval = tick;
			timer.Elapsed += (s, e) => room.Update();
			timer.AutoReset = true;
			timer.Enabled = true;

			_timers.Add(timer); //나중에 타이머 종료를 위해 리스트에 추가
			//timer.Stop(); //종료하고 싶으면 호출
        }

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			//TEST CODE
			using(AppDbContext db = new AppDbContext())
			{
                PlayerDb playerDb = db.Players.FirstOrDefault();
				if (playerDb != null)
				{
					db.Items.Add(new ItemDb()
					{
                        TemplateId = 1,
                        Count = 1,
						Slot = 0,
                        Owner = playerDb,
                    });
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 100,
                        Count = 1,
                        Slot = 1,
                        Owner = playerDb,
                    });
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 101,
                        Count = 1,
                        Slot = 2,
                        Owner = playerDb,
                    });
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 200,
                        Count = 1,
                        Slot = 5,
                        Owner = playerDb,
                    });

					db.SaveChanges();
                }
            }

			GameRoom room = RoomManager.Instance.Add(1);
			TickRoom(room, 50);	//50ms마다 업데이트

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			//FlushRoom();
			//JobTimer.Instance.Push(FlushRoom);

			//종료되지 않게끔 대기
			while (true)
			{
				DbTransaction.Instance.Flush();
			}
		}
	}
}
