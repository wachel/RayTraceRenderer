using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using UnityEngine;
using NetFrame;
using System.IO;


public class NetServer
{
    public Action<long> OnClientConnected;
    public Action<long> OnClientDisconnected;
    public Action<long,byte[]> OnDataReceived;

    public void Start(int port)
    {
        server = new AsyncTcpServer(port);
        server.Encoding = Encoding.UTF8;
        server.OnClientConnected = MT_OnClientConnected;
        server.OnClientDisconnected = MT_OnClientDisconnected;
        server.OnDatagramReceived = MT_OnDatagramReceived;
        server.Start();
    }

    public void Tick()
    {
        lock (dataQueue) {
            while (dataQueue.Count > 0) {
                EventData evt = dataQueue.Dequeue();
                switch (evt.type) {
                    case EventType.Connect: OnClientConnected(evt.sid); break;
                    case EventType.Disconnect: OnClientDisconnected(evt.sid); break;
                    case EventType.Data: OnDataReceived(evt.sid, evt.data); break;
                    default: break;
                }
            }
        }
    }

    public void Send(long sid,byte[] data)
    {
        lock (clients) {
            TcpClientState client;
            if (clients.TryGetValue(sid, out client)) {
                server.Send(client, data);
            }
        }
    }

    public void Disconnect(long sid)
    {
        lock (clients) {
            TcpClientState client;
            if (clients.TryGetValue(sid, out client)) {
                server.Disconnect(client);
            }
        }
    }

    public void Close()
    {
        lock (clients) {
            foreach (var item in clients) {
                server.Disconnect(item.Value);
            }
        }
    }

    enum EventType
    {
        Connect,
        Disconnect,
        Data,
    }

    struct EventData
    {
        public EventData(EventType type,long sid,byte[] data)
        {
            this.type = type;
            this.sid = sid;
            this.data = data;
        }
        public EventType type;
        public long sid;
        public byte[] data;
    }
    Queue<EventData> dataQueue = new Queue<EventData>();
    Dictionary<long, TcpClientState> clients = new Dictionary<long, TcpClientState>();
    AsyncTcpServer server;

    void MT_OnClientConnected(TcpClientState client)
    {
        lock (clients) {
            clients[client.Sid] = client;
        }
        lock (dataQueue) {
            dataQueue.Enqueue(new EventData(EventType.Connect, client.Sid, null));
        }
    }

    void MT_OnClientDisconnected(TcpClientState client)
    {
        lock (dataQueue) {
            dataQueue.Enqueue(new EventData(EventType.Disconnect, client.Sid, null));
        }
        lock (clients) {
            clients.Remove(client.Sid);
        }
    }

    void MT_OnDatagramReceived(TcpClientState client, byte[] data)
    {
        lock (dataQueue) {
            dataQueue.Enqueue(new EventData(EventType.Data, client.Sid, data));
        }
    }
}
