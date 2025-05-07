using NUnit.Framework;
using UnityEngine;

public class UdpMono_BroadcastBytesOnRange255 : MonoBehaviour
{

    public Udp_BroadcastBytesOnRange255 m_broadcaster;
   

    public void PushBytes(byte[] bytes)
    {

        m_broadcaster?.PushBytes(bytes);
    }
}


