using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2; //Packet 클래스의 첫 데이터인 size가 ushort이므로 2바이트

        //[size(2)][packetId(2)][...] [size(2)][packetId(2)][...] 
        public sealed override int OnRecv(ArraySegment<byte> buffer)    //다음 상속자가 OnRecv를 쓸수없도록 sealed한다.
        {
            int processLen = 0;
            int packetCount = 0;
            while (true)
            {
                //최소한 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize)
                    break;

                //패킷이 완전체로 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                //여기까지 왔으면 패킷 조립이 가능 >> 첫 패킷을 잘라서 넘긴다.
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                packetCount++;

                processLen += dataSize; //처리한 데이터의 길이를 수정하고
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset+ dataSize, buffer.Count - dataSize); //다음 패킷 뭉텅이를 다시만든다(첫 패킷 부분을 자르기)
            }

            if(packetCount > 1)
                Console.WriteLine($"패킷 모아보내기: {packetCount}");

            return processLen;
        }
        public abstract void OnRecvPacket(ArraySegment<byte> buffer); //위에서 sealed했으니 다음 상속때는 이것을 사용하기 위함. >> 컨텐츠 단에서 패킷을 파싱해서 packetId에 따라 처리해줌
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);  //얼만큼의 데이터를 받았는지 int로 return 받도록
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            //recvArgs.UserToken = this; //넘겨주고 싶은 인자가 있을경우 사용, object type으로 숫자 등 사용 가능
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(List<ArraySegment<byte>> sendBuffList)   //동시다발적으로 send를 호출할수 있으므로 lock개념을 도입해야 한다.
        {
            if (sendBuffList.Count == 0)    //예외처리
                return;
            lock (_lock)
            {
                foreach(ArraySegment<byte> sendBuff in sendBuffList)
                    _sendQueue.Enqueue(sendBuff); //Send Que에 sendBuff 값을 넣어준다.
                if (_pendingList.Count == 0)    //pendingList에 값이 있다면 Send로 등록해준다.
                    RegisterSend();
            }
        }

        public void Send(ArraySegment<byte> sendBuff)   //동시다발적으로 send를 호출할수 있으므로 lock개념을 도입해야 한다.
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff); //Send Que에 sendBuff 값을 넣어준다.
                if (_pendingList.Count == 0)    //pendingList에 값이 있다면 Send로 등록해준다.
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        #region 네트워크 통신
        void RegisterSend()
        {
            if (_disconnected == 1) //멀티쓰레드에서 발생하는 문제 방어1
                return;
            while (_sendQueue.Count > 0)    //pendingList 를 만들기 위해 while을 돈다.
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue(); //Dequeue로 전부 꺼낸다음
                _pendingList.Add(buff); // 바로 넣어줄수 있지요!
                //아래는 byte[] >> ArraySegment<byte> 로 변경되어 삭제
                //byte[] buff = _sendQueue.Dequeue(); //Dequeue로 전부 꺼낸다음
                //_pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length)); //  _sendArgs.BufferList에 바로 넣어주지 않는 이유는 구조체 자체가 할당(다음 라인)으로 넘겨줘야 하는 구조로 만들어져 있어서이다.
            }
            _sendArgs.BufferList = _pendingList;    //while문에서 다른 변수에 담은뒤에 다시 넣는 이유는 MSDN 가이드에도 없지만 이렇게 해주어야 정상적으로 처리가 된다.

            try     //멀티쓰레드에서 발생하는 문제 방어2
            {
                bool pending = _socket.SendAsync(_sendArgs);    //SendAsync로 보내준다. >> 핸들러에 의해 OnSendCompleted()가 실행된다.
                if (pending == false)       //혹시나 Send중이지 않다면
                    OnSendCompleted(null, _sendArgs);   //직접 보내준다
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;    //끝났으므로 초기화
                        _pendingList.Clear();           //끝났으므로 초기화

                        OnSend(_sendArgs.BytesTransferred);
                        

                        if (_sendQueue.Count > 0)   //혹시나 큐에 쌓인것이 있다면 같이 보내주자
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed - {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }
        void RegisterRecv()
        {
            if (_disconnected == 1) //멀티쓰레드에서 발생하는 문제 방어1
                return;
            _recvBuffer.Clean();    //초기화 해주고
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;  //write할수 있는 버퍼만큼만 획득해서
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);  //버퍼를 지정해준다.

            try   //멀티쓰레드에서 발생하는 문제 방어2
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)   //운이 좋아서 데이터를 받는 중이 아니므로 바로 실행
                    OnRecvCompleted(null, _recvArgs);
                //else인 경우 즉 데이터를 받는중이면, 이벤트 핸들러가 다음에 OnRecvCompleted()를 실행시켜 줄거다.
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed {e}");
                throw;
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //Write커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)    //write가 잘됫는지 체크했는데 안됫으면
                    {
                        Disconnect(); //끊어버리고 종료
                        return;
                    }

                    //컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다.
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen == 0 || _recvBuffer.DataSize < processLen)   //예외처리
                    {
                        Disconnect();
                        return;
                    }

                    //Read 커서 이동
                    if (_recvBuffer.OnRead(processLen) == false)    //예외처리  >> 조건문으로 썻는데도 실행은 된다.(read 커서이동)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed - {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
