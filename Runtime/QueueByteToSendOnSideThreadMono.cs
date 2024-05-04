using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class QueueByteToSendOnSideThreadMono : MonoBehaviour
{

    public List<string> m_targetAddresses = new List<string>();
    public System.Threading.ThreadPriority m_threadPriority =
        System.Threading.ThreadPriority.Normal;

    public QueueByteToSendOnSideThread m_sendThread;

    public int m_targetCount;
    public int m_messageInQueueCount;

    public void EnqueueGivenRef(byte[] toPushBytes)
    {
        m_sendThread.EnqueueGivenRef(toPushBytes);
    }
    public void EnqueueGivenAsCopy(byte[] toPushBytes)
    {
        m_sendThread.EnqueueGivenAsCopy(toPushBytes);
    }

    private void Update()
    {
        m_messageInQueueCount = m_sendThread.m_waitingBytes.Count;
        m_targetCount = m_targetAddresses.Count;
    }
    public void Awake()
    {
        m_sendThread = new QueueByteToSendOnSideThread(m_threadPriority);
        foreach (var item in m_targetAddresses)
        {
            m_sendThread.TryToAddAddress(item);
        }
    }

    public void OnDestroy()
    {
        m_sendThread.StopThread();
    }

    public void Reset()
    {
        m_targetAddresses.Add("127.0.0.1:4657");
    }
}


public class QueueByteToSendOnSideThread
{

    public void StopThread() { m_keepAlive = false; }


    bool m_keepAlive = true;
    Thread t;
    public Queue<byte[]> m_waitingBytes = new Queue<byte[]>();


    List<IPEndPoint> m_endpoints = new List<IPEndPoint>();


    public void TryToAddAddress(string addresseAndPort) {

        if (addresseAndPort == null) return;
        if (addresseAndPort.IndexOf(':') <= 7) return;
        string[] t = addresseAndPort.Split(":");
        if (t.Length != 2)
            return;
        if (int.TryParse(t[1], out int port)) {

            m_endpoints.Add(new IPEndPoint(IPAddress.Parse(t[0].Trim()), port));
        }
    }
    //    endpoints.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4567));

    private  void PushInQueueAndWait()
    {
        using (UdpClient client = new UdpClient())
        {
            while (m_keepAlive) {

                while (m_waitingBytes.Count > 0)
                {

                     byte[] b = m_waitingBytes.Dequeue();
                    foreach (IPEndPoint endpoint in m_endpoints)
                    {
                        client.Send(b, b.Length, endpoint);
                    }
                }
                Thread.Sleep(TimeSpan.FromTicks(1000));
            }
        }
        if(t!=null && t.IsAlive)
            t.Abort();
    }

    public void EnqueueGivenRef(byte[] toPushBytes)
    {
        m_waitingBytes.Enqueue(toPushBytes);
    }
    public void EnqueueGivenAsCopy(byte[] toPushBytes)
    {
        m_waitingBytes.Enqueue(toPushBytes.ToArray());
    }

    public QueueByteToSendOnSideThread(System.Threading.ThreadPriority priority) {
      t= new Thread(new ThreadStart(PushInQueueAndWait));
      t.Priority = priority;
      t.IsBackground = true;
      t.Start();
    }


    ~QueueByteToSendOnSideThread() {
        if (t != null && t.IsAlive)
            t.Abort();
    }
}