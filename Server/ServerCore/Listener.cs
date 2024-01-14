using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            //문지기
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            //문지기 교육
            _listenSocket.Bind(endPoint);

            //영업시작
            //backlog : 최대 대기수
            _listenSocket.Listen(backlog);

            //레지스터의 갯수만큼 문지기를 늘려준다.
            for(int i=0; i<register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptComplited);
                RegisterAccept(args);
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;   //이전단계에서 이벤트정보를 가지고 잇을수 있으니 지워준다

            bool pending = _listenSocket.AcceptAsync(args);
            // pending == false 라는 의미는 시작과 동시에 accept가 발생했다.
            if (pending == false)
                OnAcceptComplited(null, args);
        }

        void OnAcceptComplited(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            //다음을 위해 기다리기 위해 대기모드
            RegisterAccept(args);
        }
    }
}
