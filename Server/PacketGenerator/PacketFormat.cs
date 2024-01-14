using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//아래 처럼 직렬화를 할때 단순히 더해주는 방법이 아니라 더 최적화된 방법이 있지만
//인디게임 수준에서는 이정도면 되고, 최적화에는 더 고급 기법이 필요하다(ex C++) or chatGPT
//구글 플로트버퍼/FlatBuffers를 사용하면 serialize를 자동으로 할수 있지만 자동화하는 코드를 만들어 봄으로써 동작원리를 익힐수 있다.

namespace PacketGenerator
{
    public class PacketFormat
    {
        //{0} 패킷 등록
        public static string managerFormat =
@"using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager
{{
    #region Singleton   
    static PacketManager _instance = new PacketManager();
    public static PacketManager instance {{  get {{ return _instance; }} }}
    #endregion

    PacketManager()
    {{
        Register();
    }}

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
{0}        
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {{
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        //기존의 packet 종류를 switch문으로 분기해서 처리하던 방식을 handler + action 조합으로 효율화
        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (_makeFunc.TryGetValue(id, out func))
        {{
            IPacket packet = func.Invoke(session, buffer);
            if(onRecvCallback != null)  //곧바로 처리하지 않고 액션을 넣어줘서 다른 작업을 시킬수도 있게 한다.
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }}
    }}

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T pkt = new T();
        pkt.Read(buffer);
        return pkt;

        //패킷을 만들고 곧바로 핸들러로 넘겨주고 있었는데 이것을 분리하자 >> HandlePacket()로 분리
        //분리해서 패킷을 만들 후에는 곧바로 패킷큐에 넣고
        //유니티 메인 쓰레드를 가진 네트워크 매니저의 업데이트 부분에서 패킷큐에 있는 것을 꺼내서 핸들러에 넘겨주는 것을 하자.
        //Action<PacketSession, IPacket> action = null;
        //if (_handler.TryGetValue(pkt.Protocol, out action))
        //    action.Invoke(session, pkt);
    }}

    public void HandlePacket(PacketSession session, IPacket pkt)
    {{
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }}
}}";

        //{0} 패킷 이름
        public static string managerRegisterFormat =
@"        _makeFunc.Add((ushort)PacketID.{0}, MakePacket<{0}>);
		_handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);";

        //{0} 패킷 이름/번호 목록
        //{1} 패킷 목록
        public static string fileFormat =
@"using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public enum PacketID
{{
    {0}
}}
public interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}
{1}
";
        //{0} 패킷 이름
        //{1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";


        //{0} 패킷이름
        //{1} 멤버 변수들
        //{2} 멤버 변수 Read
        //{3} 멤버 변수 Write
        public static string packetFormat =
@"
public class {0} : IPacket
{{
    {1}

    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);
        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        
        count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.{0}), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		{3}

        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));
        
        return SendBufferHelper.Close(count);
    }}
}}
";
        //{0} 변수 형식
        //{1} 변수 이름
        public static string memberFormat =
@"public {0} {1};";

        //{0} 리스트 이름 [대문자]
        //{1} 리스트 이름 [소문자]
        //{2} 멤버 변수들
        //{3} 멤버 변수 Read
        //{4} 멤버 변수 Write
        public static string memberListFormat =
@"
public class {0}
{{
    {2}
    
    public void Read(ArraySegment<byte> segment, ref ushort count)
    {{
        {3}
    }}
    public bool Write(ArraySegment<byte> segment, ref ushort count)
    {{
        bool success = true;
        {4}
        return success;
    }}
}}
public List<{0}> {1}s = new List<{0}>();
";

        //{0} 변수 이름
        //{1} To~ 변수 형식
        //{2} 변수 형식
        public static string readFormat =
@"this.{0} = BitConverter.{1}(segment.Array, segment.Offset + count);
count += sizeof({2});";

        //{0} 변수 이름
        //{1} 변수 형식
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count+=sizeof({1});";

        //{0} 변수 이름
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(segment.Array, segment.Offset + count, {0}Len);
count += {0}Len;";

        //{0} 리스트 이름 [대문자]
        //{1} 리스트 이름 [소문자]
        public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
	{0} {1} = new {0}();
	{1}.Read(segment, ref count);
	{1}s.Add({1});
}}";

        //{0} 변수 이름
        //{1} 변수 형식
        public static string writeFormat =
@"Array.Copy(BitConverter.GetBytes(this.{0}), 0, segment.Array, segment.Offset + count, sizeof({1}));
count += sizeof({1});";

        //{0} 변수 이름
        //{1} 변수 형식
        public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1});";

        //{0} 변수 이름
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));//nameLen을 먼저 넣어줘야 하는데, 그렇게 할수 없으니 ushort두자리를 비워두고 name을 먼저 넣는다.
Array.Copy(BitConverter.GetBytes({0}Len), 0, segment.Array, segment.Offset + count, sizeof(ushort));
count += sizeof(ushort);
count += {0}Len;";

        //{0} 리스트 이름 [대문자]
        //{1} 리스트 이름 [소문자]
        public static string writeListFormat =
@"Array.Copy(BitConverter.GetBytes((ushort)this.{1}s.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));
count += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
	{1}.Write(segment, ref count);";

    }
}
