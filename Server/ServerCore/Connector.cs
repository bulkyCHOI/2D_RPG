using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
	public class Connector
	{
		Func<Session> _sessionFactory;

		public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				// 휴대폰 설정
				Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				_sessionFactory = sessionFactory;

				SocketAsyncEventArgs args = new SocketAsyncEventArgs();
				args.Completed += OnConnectCompleted;
				args.RemoteEndPoint = endPoint;
				args.UserToken = socket;

				RegisterConnect(args);

				//TEMP
				Thread.Sleep(10);	// 10ms	DummyClient에서는 한번에 모든 유저가 몰리는데
									// Listener에서는 backog = 100으로 설정해두어 한번 대기자가 최대 100명이다.
									// 그러므로 10ms를 쉬게해서 튕기는 일 없이 모두 들어오게 하기 위함
			}
		}

		void RegisterConnect(SocketAsyncEventArgs args)
		{
			Socket socket = args.UserToken as Socket;
			if (socket == null)
				return;

			try
			{
				bool pending = socket.ConnectAsync(args);
				if (pending == false)
					OnConnectCompleted(null, args);
			}
			catch (Exception e)
			{
                Console.WriteLine($"RegisterConnect Failed {e}");
            }
		}

		void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
		{
			try
			{
				if (args.SocketError == SocketError.Success)
				{
					Session session = _sessionFactory.Invoke();
					session.Start(args.ConnectSocket);
					session.OnConnected(args.RemoteEndPoint);
				}
				else
				{
					Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
				}
			}
			catch (Exception e)
			{
                Console.WriteLine($"OnConnectCompleted Failed {e}");
            }
		}
	}
}
