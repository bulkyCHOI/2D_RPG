using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper   //사용하기 편리하게 헬퍼 클래스를 만들자(ThreadLocal)
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(()=> { return null; });    //전역변수 대신 ThreadLocal로 멀티쓰레드 각자가 독립되게 사용할수 잇도록 설정 >> 전역변수로 사용하게 되면 멀티쓰레드 환경에서 race condition을 유발하게 된다.

        public static int ChunkSize { get; set; } = 65535 * 100;
        public static ArraySegment<byte> Open(int reserveSize) //넉넉한 공간을 잡아서 열고
        {
            if (CurrentBuffer.Value == null)    //비어있다 == 아직 생성이 안됫다.
                CurrentBuffer.Value = new SendBuffer(ChunkSize);    //chunksize 크기로 하나 만들어주자

            if(CurrentBuffer.Value.FreeSize < reserveSize)  //남아있는 사이즈가 예약 사이즈보다 작아서 공간이 모자르면
                CurrentBuffer.Value = new SendBuffer(ChunkSize);    //chunksize 크기로 새롭게 밀어버리자

            return CurrentBuffer.Value.Open(reserveSize);
        }
        public static ArraySegment<byte> Close(int usedSize)   //쓴만큼만 _usedSize를 더한다.
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer   //데이터를 보낼때 큰 덩어리를 조각내어 보내고 싶은 만큼만 보내고 나머지는 남겨둔다.
    {
        byte[] _buffer;
        int _usedSize = 0;

        public int FreeSize { get { return _buffer.Length - _usedSize; } }  //사용가능한 버퍼 사이즈

        public SendBuffer(int chunkBuffer)
        {
            _buffer= new byte[chunkBuffer];
        }

        public ArraySegment<byte> Open(int reserveSize) //넉넉한 공간을 잡아서 열고
        {
            if (reserveSize > FreeSize)
                return null;
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }
        public ArraySegment<byte> Close(int usedSize)   //쓴만큼만 _usedSize를 더한다.
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
