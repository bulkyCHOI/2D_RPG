using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Collections.Specialized.BitVector32;
using ServerCore;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.instance.Push(FlushRoom, 250);
        }

        static void Main(string[] args)
        {
            //DNS Domain Name System
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 80);

            _listener.Init(endPoint, () => { return SessionManager.instance.Generate(); });
            Console.WriteLine("Listening...");

            JobTimer.instance.Push(FlushRoom);

            while (true)
            {
            JobTimer.instance.Flush();

            }
        }
    }
}