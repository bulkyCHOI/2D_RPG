using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager
{
    #region Singleton   
    static PacketManager _instance = new PacketManager();
    public static PacketManager instance {  get { return _instance; } }
    #endregion

    PacketManager()
    {
        Register();
    }

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
        _makeFunc.Add((ushort)PacketID.C_LeaveGame, MakePacket<C_LeaveGame>);
		_handler.Add((ushort)PacketID.C_LeaveGame, PacketHandler.C_LeaveGameHandler);        _makeFunc.Add((ushort)PacketID.C_Move, MakePacket<C_Move>);
		_handler.Add((ushort)PacketID.C_Move, PacketHandler.C_MoveHandler);        
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        //기존의 packet 종류를 switch문으로 분기해서 처리하던 방식을 handler + action 조합으로 효율화
        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (_makeFunc.TryGetValue(id, out func))
        {
            IPacket packet = func.Invoke(session, buffer);
            if(onRecvCallback != null)  //곧바로 처리하지 않고 액션을 넣어줘서 다른 작업을 시킬수도 있게 한다.
                onRecvCallback.Invoke(session, packet);
            else
                HandlePacket(session, packet);
        }
    }

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T pkt = new T();
        pkt.Read(buffer);
        return pkt;

        //패킷을 만들고 곧바로 핸들러로 넘겨주고 있었는데 이것을 분리하자 >> HandlePacket()로 분리
        //분리해서 패킷을 만들 후에는 곧바로 패킷큐에 넣고
        //유니티 메인 쓰레드를 가진 네트워크 매니저의 업데이트 부분에서 패킷큐에 있는 것을 꺼내서 핸들러에 넘겨주는 것을 하자.
        //Action<PacketSession, IPacket> action = null;
        //if (_handler.TryGetValue(pkt.Protocol, out action))
        //    action.Invoke(session, pkt);
    }

    public void HandlePacket(PacketSession session, IPacket pkt)
    {
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }
}