using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ClientSession : PacketSession
    {
        public int SessionID { get; set; }
        public GameRoom Room { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");
            Program.Room.Push(
                () => Program.Room.Enter(this));
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.instance.Remove(this);
            if(Room != null)
            {
                GameRoom room = Room;
                room.Push(() => room.Leave(this));
                Room = null;    //혹시라도 두번호출될 경우 대비
            }
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        //PacketSession 클래스로 변경했고, OnRecv는 sealed로 봉인했기 때문에 사용 불가.
        //public override int OnRecv(ArraySegment<byte> buffer)
        //{
        //    string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        //    Console.WriteLine($"[From Client] {recvData}");
        //    return buffer.Count;    //받은 버퍼만큼을 리턴해준다.
        //}

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
