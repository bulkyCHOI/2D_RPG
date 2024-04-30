using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;

public class NetworkManager
{
    public static string GameServerAddress { get; set; } = "61.73.10.119";
    public static string AccountServerAddress { get; set; } = "61.73.10.119";

    public int AccountId { get; set; }
    public string AccountName { get; set; }
    public int Token { get; set; } 

    ServerSession _session = new ServerSession();

    public void Send(IMessage packet)
    {
        _session.Send(packet);
    }

    public void ConnectToServer(string host, int port)
    {
        try
        {
            IPAddress ipAddr = IPAddress.Parse(host);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

            Connector connector = new Connector();

            connector.Connect(endPoint,
                               () => { return _session; },
                                              1);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
    public void ConnectToGame(ServerInfo serverInfo)
    {
        // DNS (Domain Name System)
        //string host = Dns.GetHostName();
        //IPHostEntry ipHost = Dns.GetHostEntry(host);
        //IPAddress ipAddr = ipHost.AddressList[1];

        IPAddress ipAddr = IPAddress.Parse(serverInfo.ServerIp);
        IPEndPoint endPoint = new IPEndPoint(ipAddr, serverInfo.ServerPort);

        Connector connector = new Connector();

        connector.Connect(endPoint,
            () => { return _session; },
            1);
    }

    public void Update()
    {
        List<PacketMessage> list = PacketQueue.Instance.PopAll();
        foreach (PacketMessage packet in list)
        {
            Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_session, packet.Message);
        }
    }

}
