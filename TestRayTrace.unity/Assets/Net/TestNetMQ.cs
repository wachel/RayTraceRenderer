using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;


public class NetMqPublisher2
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly System.Diagnostics.Stopwatch _contactWatch;

    private const long ContactThreshold = 1000;

    public bool Connected;

    private object queueLock = new object();
    private Queue<string> messageQueue = new Queue<string>();

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var server = new ResponseSocket()) {
            server.Bind("tcp://*:12346");

            while (!_listenerCancelled) {
                Connected = _contactWatch.ElapsedMilliseconds < ContactThreshold;
                string message;
                if (!server.TryReceiveFrameString(out message)) continue;
                _contactWatch.Start();
                OnReceiveMessage_Thread(message);
                server.SendFrame("ok");
            }
        }
        NetMQConfig.Cleanup();
    }

    public void OnReceiveMessage_Thread(string msg)
    {
        lock (queueLock) {
            messageQueue.Enqueue(msg);
        }
    }

    public void Tick()
    {
        lock (queueLock) {
            while(messageQueue.Count > 0) {
                _messageDelegate(messageQueue.Dequeue());
            }
        }
    }

    public NetMqPublisher2(MessageDelegate messageDelegate)
    {
        _contactWatch = new System.Diagnostics.Stopwatch();
        _contactWatch.Start();
        _listenerWorker = new Thread(ListenerWork);
    }

    public void Start(int port)
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}

public class TestNetMQ : MonoBehaviour
{
    public bool Connected;
    private NetMqPublisher2 _netMqPublisher;

    private void Start()
    {
        _netMqPublisher = new NetMqPublisher2(HandleMessage);
        _netMqPublisher.Start(12346);
    }

    private void Update()
    {
        var position = transform.position;
        Connected = _netMqPublisher.Connected;
        _netMqPublisher.Tick();
    }

    private void HandleMessage(string message)
    {
        Debug.Log(message);
    }

    private void OnDestroy()
    {
        _netMqPublisher.Stop();
    }
}

