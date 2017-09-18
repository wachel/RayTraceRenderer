using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;
using NetMQ.Sockets;
using NetMQ;

public class TestMQVentilator : MonoBehaviour
{
    NetMQSocket response;

    NetMQSocket sender;
    NetMQSocket receiver;

    void Start()
    {
        AsyncIO.ForceDotNet.Force();

        response = new ResponseSocket();
        response.Bind("tcp://127.0.0.1:5555");

        sender = new DealerSocket();
        sender.Bind("tcp://127.0.0.1:5557");

        receiver = new DealerSocket();
        receiver.Bind("tcp://127.0.0.1:5558");
    }

    public void OnApplicationQuit()
    {
        receiver.Dispose();
        sender.Dispose();
        response.Dispose();
        NetMQConfig.Cleanup();
    }
    
    void Update()
    {
        //sender.TrySendFrame("work");
        //string temp;
        //if (receiver.TryReceiveFrameString(out temp)) {
        //    Debug.Log(temp);
        //}
        string temp2;
        if(response.TryReceiveFrameString(out temp2)) {
            Debug.Log(temp2);
            response.TrySendFrame("ok");
        }

    }
}
