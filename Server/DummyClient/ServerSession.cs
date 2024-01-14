using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    class ServerSession : PacketSession
    {
        //아래 함수를 사용하면 C++ 코드처럼 포인터를 사용하여 Byte전환이 가능하다.(아래 TryWriteBytes()가 유니티에서 실행이 안된다면 사용)
        //static unsafe void ToBytes(byte[] array, int offset, ulong value)
        //{
        //    fixed(byte* ptr = &array[offset])
        //    {
        //        *(ulong*)ptr = value;
        //    }
        //}
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"Transfered bytes: {numOfBytes}");
        }
    }
}
