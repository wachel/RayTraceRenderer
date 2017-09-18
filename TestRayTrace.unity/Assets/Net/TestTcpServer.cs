using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TestTcpServer : MonoBehaviour
{
    NetServer server;
	// Use this for initialization
	void Start () {
        server = new NetServer();
        server.Start(9999);
        server.OnClientConnected = OnClientConnected;
        server.OnClientDisconnected = OnClientDisconnected;
        server.OnDataReceived = OnDataReceived;
    }

    void OnClientConnected(long sid)
    {
        Debug.LogFormat("client connected,sid= {0}", sid);
    }

    void OnClientDisconnected(long sid)
    {
        Debug.LogFormat("client disconnected,sid = {0}", sid);
    }

    void OnDataReceived(long sid,byte[] data)
    {
        Debug.LogFormat("received sid={0},len={1}", sid, data);
        server.Send(sid, Encoding.UTF8.GetBytes("aaaaaaaa"));
    }

    void Update () {
        server.Tick();
	}
}
