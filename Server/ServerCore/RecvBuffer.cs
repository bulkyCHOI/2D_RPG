using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer     //TCP 패킷의 특징을 살려 전송이 완료된 것만 미리 선처리 해서 처리 속도 향상을 위한 클래스
    {
        // [r][][][][w][][][][][]
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int buffersize)
        {
            _buffer = new ArraySegment<byte>(new byte[buffersize], 0, buffersize);
        }

        public int DataSize { get { return _writePos - _readPos; } }    //데이터가 들어 있는 버퍼의 사이즈
        public int FreeSize { get { return _buffer.Count - _writePos; } } //남아있는 버퍼의 사이즈

        public ArraySegment<byte> ReadSegment   //read할 수 있는 유효 범위
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }
        public ArraySegment<byte> WriteSegment  //write할 수 있는 유효 범위
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean() //버퍼를 정리해주는 함수, 정리하지 않고 계속 사용하면 앞에서 뒤로 read/writePos만 이동되고 여유공간은 확보해주지 못하므로 부족하게 됨
        {
            int dataSize = DataSize;
            if (dataSize == 0)  //남은 데이터가 없으면 복사하지 않고 커서 위치만 처음으로 리셋
            {
                _readPos = _writePos = 0;
            }
            else                //남은 데이터가 있으면, 데이터를 처음으로 복사하고, read/writePos 옮기기
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes) //Read가 성공적으로 완료되었을때 readPos를 이동시키는 함수
        {
            if (numOfBytes > DataSize)  //예외처리 해주고
                return false;
            _readPos += numOfBytes;
            return true;
        }
        public bool OnWrite(int numOfBytes) //write가 성공적으로 완료되었을때 writePos를 이동시키는 함수
        {
            if(numOfBytes > FreeSize) //예외처리 해주고
                return false;
            _writePos += numOfBytes;
            return true;
        }
    }
}
