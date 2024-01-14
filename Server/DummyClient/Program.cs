using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //DNS Domain Name System
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 80);

            Connector connector = new Connector();

            connector.Connect(endPoint, 
                () => { return SessionManager.Instance.Generate(); }, 
                600);
            
            while (true)
            {
                try
                {
                    SessionManager.Instance.SendForEach(); //채팅메시지를 모두에게 날려주는 함수
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                Thread.Sleep(3000);  //일반적으로 MMO에서 이동패킷이 1초에 4번 전송되므로, 채팅을 통해 시뮬레이션이 잘 되는지 테스트 해보기 위함.
            }
        }
    }
}