using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    internal class SessionManager
    {
        //싱글톤 패턴처럼 만들기
        static SessionManager _instance = new SessionManager();
        public static SessionManager Instance { get {  return _instance; } }

        List<ServerSession> _sessions = new List<ServerSession>();
        object _lock = new object();
        Random _random = new Random();  //더미쪽 케릭터들의 좌표를 랜덤으로 주기위해서

        public void SendForEach()   //모든 클라이언트들에서 채팅 메시지를 전송하는 함수
        {
            lock (_lock)
            {
                foreach (ServerSession session in _sessions) //생성된 더미 세션들마다 랜덤 좌표값을 넣어 전송
                {
                    C_Move movePacket = new C_Move();
                    movePacket.posX = _random.Next(-50, 50);
                    movePacket.posY = _random.Next(0, 10);
                    movePacket.posZ = _random.Next(-50, 50);
                    session.Send(movePacket.Write());
                }
            }
        }

        public ServerSession Generate()
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                _sessions.Add(session);
                return session;
            }
        }
    }
}
