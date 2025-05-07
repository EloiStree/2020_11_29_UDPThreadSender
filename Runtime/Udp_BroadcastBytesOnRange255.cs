using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

[System.Serializable]
public class Udp_BroadcastBytesOnRange255 {

    public List<TargetIpPort> m_addresses = new List<TargetIpPort>();
    public int m_minRange = 200;
    public int m_maxRange = 255;
    public int[] m_specificTarget255 = new int[] { 112 };
    public ulong m_sendCount = 0;


    [ContextMenu("Push Random Integer For Test")]
    public void PushRandomIntegerForTest()
    {
        int randomInt = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        byte[] bytes = BitConverter.GetBytes(randomInt);
        PushBytes(bytes);
    }
    public void PushBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return;
        }
        foreach (var address in m_addresses)
        {
            if (address.m_ip == null || address.m_ip.Length == 0)
            {
                continue;
            }
            string[] ipParts = address.m_ip.Split('.');
            if (ipParts.Length == 4)
            {
                string threeFirst = string.Join(".", ipParts[0], ipParts[1], ipParts[2])+".";
                for (int i = m_minRange; i <= m_maxRange; i++)
                {
                    string ip = threeFirst + i.ToString();
                    PushBytesTo(ip, address.m_port, bytes);
                }
            }
        }
    }

    private void PushBytesTo(string ip, int port, byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return;
        }

        UdpClient target = new UdpClient(ip, port);
        target.Send(bytes, bytes.Length);
        target.Close();
        m_sendCount+= (ulong) bytes.Length;
    }
}


