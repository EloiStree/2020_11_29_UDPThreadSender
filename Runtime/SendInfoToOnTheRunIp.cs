using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendInfoToOnTheRunIp : MonoBehaviour
{

    public UDPThreadSender m_sender;
    public string[] m_addressesGiven;
    public TargetIpPort[] m_addressesIpGiven;

    public void SendTextToTargets(string text) {

        for (int i = 0; i < m_addressesIpGiven.Length; i++)
        {
            m_sender.SendMessageTo(m_addressesIpGiven[i], text);
        }

    }
    public void SetAddressesSplitBy(string text)
    {
        SetAddressesSplitBy(text, ' ');
    }
        public void SetAddressesSplitBy(string text, char spliter=' ') {

        m_addressesGiven =  text.Split(spliter);
        m_addressesIpGiven = new TargetIpPort[m_addressesGiven.Length];
        for (int i = 0; i < m_addressesGiven.Length; i++)
        {
            m_addressesGiven[i]=m_addressesGiven[i].Trim();
            string[] token = m_addressesGiven[i].Split(':');
            if (token.Length == 2 && int.TryParse(token[1], out int port)) {
                m_addressesIpGiven[i] = new TargetIpPort(token[0], port);
            }
        }
    }

}
