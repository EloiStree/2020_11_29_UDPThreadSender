using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class UDPThreadDispatcherText : MonoBehaviour
{
    public int m_portId = 2504;
    public float m_timeBetweenUnityCheck=0.05f;
    public UnityEvent<string> m_messageReceived;
    public System.Threading.ThreadPriority m_threadPriority;

    public Queue<string> m_receivedMessages = new Queue<string>();
    public string m_lastReceived;
    private bool m_wantThreadAlive = true;
    private Thread m_threadListener=null;
    public UdpClient m_listener;
    public IPEndPoint m_ipEndPoint;
    public bool m_hasBeenKilled;

    public TextType m_textType = TextType.UTF8;
    public enum TextType { UTF8, Unicode}

    public float m_timeBetweenStartThread = 0.1f;
    private IEnumerator Start()
    {
        InvokeRepeating("PushOnUnityThreadMessage", 0, m_timeBetweenUnityCheck);

        yield return new WaitForSeconds(m_timeBetweenStartThread);
        if (m_threadListener == null) { 
            m_threadListener = new Thread(ChechUdpClientMessageInComing);
            m_threadListener.Priority = m_threadPriority;
            m_threadListener.Start();
        }
    }

    public void SetPortBeforeStart(int port)
    {
        m_portId = port;
    }
    public void OnDisable()
    {
        if (!m_hasBeenKilled)
        {
            Kill();
        }

    }
    private void OnDestroy()
    {
        if (!m_hasBeenKilled)
        {
            Kill();
        }
    }
    private void OnApplicationQuit()
    {
        if (!m_hasBeenKilled)
        {
            Kill();
        }
    }

    [ContextMenu("Force Kill")]
    public void ForceKill() {
        Kill();
    }
    private void Kill()
    {
        if(m_listener!=null)
            m_listener.Close();
        if (m_threadListener != null)
            m_threadListener.Abort();
        m_wantThreadAlive = false;
        m_hasBeenKilled = true;
    }

   

    public void PushOnUnityThreadMessage() {
        while (m_receivedMessages.Count > 0) { 
            m_lastReceived = m_receivedMessages.Dequeue();
            m_messageReceived.Invoke(m_lastReceived);
        }
    }

    private void ChechUdpClientMessageInComing() {

        if (m_listener == null) { 
            m_listener = new UdpClient(m_portId);
            m_ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        while (m_wantThreadAlive) { 
            try
            {

                Byte[] receiveBytes = m_listener.Receive(ref m_ipEndPoint);
                string returnData =
                    m_textType== TextType.UTF8? 
                    Encoding.UTF8.GetString(receiveBytes):
                    Encoding.Unicode.GetString(receiveBytes);
                m_receivedMessages.Enqueue(returnData);
                //RemoteIpEndPoint.Address.ToString() --  RemoteIpEndPoint.Port.ToString());
            }
            catch (Exception e)
            {
               Debug.Log(e.ToString());
                m_wantThreadAlive = false;
            }
        }
    }


}

