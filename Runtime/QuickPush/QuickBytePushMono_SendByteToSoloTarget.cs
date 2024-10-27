using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class QuickBytePushMono_SendByteToSoloTarget : MonoBehaviour
{
    public string m_targetAddress = "127.0.0.1:3617";
    

    public void PushByteNow(byte[] toPushBytes)
    {
        UdpClient client = new UdpClient();
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(m_targetAddress.Split(':')[0]), int.Parse(m_targetAddress.Split(':')[1]));
        client.Send(toPushBytes, toPushBytes.Length, endPoint);
        client.Close();
    }
}
