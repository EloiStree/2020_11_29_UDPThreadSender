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
    public float m_startDelay = 0.1f;

    public void FlushTargets()
    {
        m_targetAddresses.Clear();
    }
    public void AddTarget(string target)
    {
        m_targetAddresses.Add(target);
    }
    public void AddTargets(IEnumerable<string> targets)
    {
        m_targetAddresses.AddRange(targets);
    }
    public void AddTargetsFromTextLineSplit(string text)
    {
        m_targetAddresses.AddRange(text.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0));
    }

    public void SetWithOnePort(int port)
    {
        m_targetAddresses.Clear();
        m_targetAddresses.Add("127.0.0.1:" + port);
    }

    public void EnqueueGivenRef(byte[] toPushBytes)
    {
        m_sendThread.EnqueueGivenRef(toPushBytes);
    }
    public void EnqueueGivenAsCopy(byte[] toPushBytes)
    {
        m_sendThread.EnqueueGivenAsCopy(toPushBytes);
    }

    public ulong m_runningTick;
    private void Update()
    {
        if (m_sendThread == null)
        {
            return;
        }
        if (m_sendThread.m_waitingBytes == null)
        {
            return;
        }
        m_sendThread.m_runningTick = m_runningTick;


        m_messageInQueueCount = m_sendThread.m_waitingBytes.Count;
        m_targetCount = m_targetAddresses.Count;
    }
    public IEnumerator Start()
    {
        yield return new WaitForSeconds(m_startDelay);
        m_sendThread = new QueueByteToSendOnSideThread(m_threadPriority);
        foreach (var item in m_targetAddresses)
        {
            m_sendThread.TryToAddAddress(item);
        }
    }

    public void ReplaceTargetWithText(string text)
    {
        m_targetAddresses.Clear();  
        AddTargetsFromTextLineSplit(text);
        foreach (var item in m_targetAddresses)
        {
            m_sendThread.TryToAddAddress(item);
        }
    }

    public void RefreshTargetsAddressToThread()
    {
        if (m_sendThread != null)
        {
            m_sendThread.ClearEndPoints();
            foreach (var item in m_targetAddresses)
            {
                m_sendThread.TryToAddAddress(item);
            }

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
    public ulong m_runningTick;
    public void StopThread() { m_keepAlive = false; }


    bool m_keepAlive = true;
    Thread t;
    public Queue<byte[]> m_waitingBytes = new Queue<byte[]>();


    List<IPEndPoint> m_endpoints = new List<IPEndPoint>();

    public void ClearEndPoints()
    {
        m_endpoints.Clear();
    }
    public void TryToAddAddress(string addresseAndPort)
    {

        if (addresseAndPort == null) return;
        if (addresseAndPort.IndexOf(':') <= 7) return;
        string[] t = addresseAndPort.Split(":");
        if (t.Length != 2)
            return;
        if (int.TryParse(t[1], out int port))
        {

            m_endpoints.Add(new IPEndPoint(IPAddress.Parse(t[0].Trim()), port));
        }
    }
    //    endpoints.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4567));

    private void PushInQueueAndWait()
    {
        using (UdpClient client = new UdpClient())
        {
            while (m_keepAlive)
            {

                while (m_waitingBytes.Count > 0)
                {

                    byte[] b = m_waitingBytes.Dequeue();
                    foreach (IPEndPoint endpoint in m_endpoints)
                    {
                        client.Send(b, b.Length, endpoint);
                    }
                }
                m_runningTick = (ulong)DateTime.Now.Ticks;
                Thread.Sleep(TimeSpan.FromTicks(1000));
            }
        }
        if (t != null && t.IsAlive)
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

    public QueueByteToSendOnSideThread(System.Threading.ThreadPriority priority)
    {
        t = new Thread(new ThreadStart(PushInQueueAndWait));
        t.Priority = priority;
        t.IsBackground = true;
        t.Start();
    }


    ~QueueByteToSendOnSideThread()
    {
        if (t != null && t.IsAlive)
            t.Abort();
    }
}