using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Server
{
    NetServer server;
    List<long> clients = new List<long>();

    void Start()
    {
        server = new NetServer();
        server.Start(9999);
        server.OnClientConnected = OnClientConnected;
        server.OnClientDisconnected = OnClientDisconnected;
        server.OnDataReceived = OnDataReceived;
    }

    void OnClientConnected(long sid)
    {
        clients.Add(sid);
    }

    void OnClientDisconnected(long sid)
    {
        clients.Remove(sid);
    }

    void OnDataReceived(long sid, byte[] data)
    {
        MsgBase msg = MessageUnpacker.Unpack(data);
    }

    void Send(long sid,MsgBase msg)
    {
        byte[] data = MessagePacker.Pack(msg);
        server.Send(sid, data);
    }

    void Tick()
    {
        server.Tick();
    }
}
