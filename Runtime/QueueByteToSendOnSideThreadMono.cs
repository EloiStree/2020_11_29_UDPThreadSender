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
    public int m_targetEndPointCount;
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
        if (m_sendThread != null)
            m_sendThread.EnqueueGivenRef(toPushBytes);
    }
    public void EnqueueGivenAsCopy(byte[] toPushBytes)
    {
        if (m_sendThread != null)
            m_sendThread.EnqueueGivenAsCopy(toPushBytes);
    }

    public ulong m_runningTick;
    private void Update()
    {
        if (m_sendThread == null)
        {
            return;
        }
       
        m_sendThread.m_runningTick = m_runningTick;


        m_messageInQueueCount = m_sendThread.GetWaitingBytes();
        m_targetCount = m_targetAddresses.Count;
        m_targetEndPointCount = m_sendThread.m_endpoints.Count;
    }
    public IEnumerator Start()
    {
        yield return new WaitForSeconds(m_startDelay);
        Debug.Log("Starting Thread UDP Pusher");
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
        if (m_sendThread != null)
            m_sendThread.StopThread();
    }

    public void Reset()
    {
        m_targetAddresses.Add("127.0.0.1:4657");
    }
}

[System.Serializable]
public class QueueByteToSendOnSideThread
{
    public ulong m_runningTick;
    public void StopThread() { m_keepAlive = false; }


    bool m_keepAlive = true;
    Thread t;
    private Queue<byte[]> m_waitingBytes = new Queue<byte[]>();


    public List<IPEndPoint> m_endpoints = new List<IPEndPoint>();

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

    public string m_lastSent="";
    public ulong m_sentByteCount;
    public long m_sentTime;
    private void PushInQueueAndWait()
    {
        m_runningTick = (ulong)DateTime.Now.Ticks;
        using (UdpClient client = new UdpClient())
        {
            while (m_keepAlive)
            {

                while (m_waitingBytes.Count > 0)
                {
                    byte[] b = m_waitingBytes.Dequeue();
                    foreach (IPEndPoint endpoint in m_endpoints)
                    {
                        m_sentByteCount += (ulong)b.Length;
                        m_lastSent= endpoint.ToString();
                        m_sentTime = DateTime.Now.Ticks;
                        client.Send(b, b.Length, endpoint);
                    }
                }
                m_runningTick = (ulong)DateTime.Now.Ticks;
                Thread.Sleep(TimeSpan.FromTicks(1000));
            }
        }
        m_runningTick = (ulong)DateTime.Now.Ticks;
        if (t != null && t.IsAlive)
            t.Abort();
        m_runningTick = (ulong)DateTime.Now.Ticks;
    }

    public void EnqueueGivenRef(byte[] toPushBytes)
    {
        m_waitingBytes.Enqueue(toPushBytes);
    }
    public void EnqueueGivenAsCopy(byte[] toPushBytes)
    {
        m_waitingBytes.Enqueue(toPushBytes.ToArray());
    }



    [ContextMenu("Push Random Integer")]
    public void PushRandomInteger() { 
    
        byte[] b = BitConverter.GetBytes(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        m_waitingBytes.Enqueue(b);
    }
    public void PushInteger(int value)
    {
        byte[] b = BitConverter.GetBytes(value);
        m_waitingBytes.Enqueue(b);
    }


    public int GetWaitingBytes()
    {
        return m_waitingBytes.Count;
    }

    public QueueByteToSendOnSideThread(System.Threading.ThreadPriority priority)
    {
        Debug.Log("A");
        t = new Thread(new ThreadStart(PushInQueueAndWait));
        t.Priority = priority;
        t.IsBackground = true;
        m_keepAlive = true;
        t.Start();
        Debug.Log("b");
    }


    ~QueueByteToSendOnSideThread()
    {
        if (t != null && t.IsAlive)
            t.Abort();
    }
}