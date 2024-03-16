using DummyClient.Session;
using ServerCore;
using System;
using System.Net;

namespace DummyClient
{
    class Program
    {
        static int DummyClientCount {get; } = 50;
        
        static void Main(string[] args)
        {
            Thread.Sleep(3000); // 서버가 먼저 켜지도록 딜레이를 준다.

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();

            connector.Connect(endPoint,
                () => { return SessionManager.Instance.Generate(); },
                Program.DummyClientCount);

            while (true)
            {
                Thread.Sleep(10000);
            }

        }
    }
}