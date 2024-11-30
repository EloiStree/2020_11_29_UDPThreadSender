using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;



public struct STRUCT_IntegerBytePackage {

    public int m_valueToSend;
}
public struct STRUCT_IndexIntegerBytePackage
{
    public int m_indexToSend;
    public int m_valueToSend;
}   
public struct STRUCT_IndexIntegerDateBytePackage
{
    public int m_indexToSend;
    public int m_valueToSend;
    public ulong m_dateToSend;
}

public class UDPThreadSender : MonoBehaviour
{
    public int m_messageInQueue=0;
    public Queue<MessageToAll> m_toSendPackageToAll = new Queue<MessageToAll>();
    public Queue<MessageToIpPort> m_toSendPackageToTarget= new Queue<MessageToIpPort>();

 

    public Queue<MessageToGroupOfAlias> m_toSendPackageWithAlias = new Queue<MessageToGroupOfAlias>();

   

    public List<TargetIpPort> m_targetRegistered = new List<TargetIpPort>();
    public List<IpSingleAlias> m_singleAlias = new List<IpSingleAlias>();
    public List<AliasToGroupOfAlias> m_groupAlias = new List<AliasToGroupOfAlias>();

    public Dictionary<string, UdpClientState> m_connections = new Dictionary<string, UdpClientState>();


    public string m_lastSent;
    [TextArea(0, 5)]
    public string m_lastException;
    public int m_maxByteDebugSize = 20;
    public byte[] m_lastBytesSend;


    public void SendDirectlyToAll(byte[] bytes)
    {
        for (int i = 0; i < m_targetRegistered.Count; i++)
        {
            SendBytesTo(m_targetRegistered[i], bytes);
        }
    }

    public void SendDirectlyToAll(STRUCT_IndexIntegerBytePackage package)
    {
        byte[] bytes = new byte[8];
        BitConverter.GetBytes(package.m_indexToSend).CopyTo(bytes, 0);
        BitConverter.GetBytes(package.m_valueToSend).CopyTo(bytes, 4);
        SendDirectlyToAll(bytes);
    }
    public void SendDirectlyToAll(STRUCT_IntegerBytePackage package)
    {
        byte[] bytes = new byte[4];
        BitConverter.GetBytes(package.m_valueToSend).CopyTo(bytes, 0);
        SendDirectlyToAll(bytes);
    }
    public void SendDirectlyToAll(STRUCT_IndexIntegerDateBytePackage package)
    {
        byte[] bytes = new byte[16];
        BitConverter.GetBytes(package.m_indexToSend).CopyTo(bytes, 0);
        BitConverter.GetBytes(package.m_valueToSend).CopyTo(bytes, 4);
        BitConverter.GetBytes(package.m_dateToSend).CopyTo(bytes, 8);
        SendDirectlyToAll(bytes);
    }
    public void SendDirectlyToAllAsByte(char charAsByte) { 
        SendDirectlyToAll(new byte[] { (byte)charAsByte });
    }
    public void SendDirectlyToAllAsByte(byte byteAsByte)
    {
        SendDirectlyToAll(new byte[] { byteAsByte });
    }
    public void SendDirectlyToAllAsByte(byte[] byteAsByte)
    {
        SendDirectlyToAll(byteAsByte);
    }
    public void SendDirectlyToAllAsInteger(int integer)
    {
        SendDirectlyToAll(BitConverter.GetBytes(integer));
    }
    public void SendDirectlyToAllAsIndexInteger(int index, int integer)
    {
        byte[] bytes = new byte[8];
        BitConverter.GetBytes(index).CopyTo(bytes, 0);
        BitConverter.GetBytes(integer).CopyTo(bytes, 4);
        SendDirectlyToAll(bytes);
    }
    public void SendDirectlyToAllStringWithLineReturn(string text)
    {
        SendDirectlyToAllAsUTF8(text + "\n");
    }   
    public void SendDirectlyToAllAsUTF8(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        SendDirectlyToAll(bytes);
    }

    public void SendDirectlyToAllStringAsIntegerElseText(string text)
    {
        int value;
        if (int.TryParse(text, out value))
        {
            SendDirectlyToAll(new STRUCT_IntegerBytePackage() { m_valueToSend = value });
        }
        else
        {
            SendDirectlyToAllAsUTF8(text);
        }
    }



    public void Clear()
    {
        m_targetRegistered.Clear();
        m_singleAlias.Clear();
        m_groupAlias.Clear();
        m_connections.Clear();
        m_toSendPackageWithAlias.Clear();
        m_toSendPackageToTarget.Clear();
        m_toSendPackageToAll.Clear();
    }
    public void SetAliasPort(string name, int port)
    {
        for (int i = 0; i < m_singleAlias.Count; i++)
        {
            if (m_singleAlias[i].m_aliasNameId == name)
                m_singleAlias[i].m_ref.m_port = port;
        }
    }
    public void SetAliasIp(string name, string ip)
    {
        for (int i = 0; i < m_singleAlias.Count; i++)
        {
            if (m_singleAlias[i].m_aliasNameId == name)
                m_singleAlias[i].m_ref.m_ip = ip;
        }
    }
    public void Add(IpSingleAlias ipSingleAlias)
    {
        if (!string.IsNullOrEmpty(ipSingleAlias.m_aliasNameId)) {
            m_singleAlias.Add(ipSingleAlias);
        }
    }

    public void Add(AliasToGroupOfAlias aliasToGroupOfAlias)
    {
        if (!string.IsNullOrEmpty(aliasToGroupOfAlias.m_alias))
        {
            m_groupAlias.Add(aliasToGroupOfAlias);
        }
    }

    public void AddMessageToSendToAll(string message)
    {
        m_toSendPackageToAll.Enqueue(new MessageToAll(message));
    }
    public void AddMessageToSendToAll(MessageToAll message)
    {
        m_toSendPackageToAll.Enqueue(message);
    }
    public void AddMessageToSend(MessageToGroupOfAlias message)
    {
        m_toSendPackageWithAlias.Enqueue(message);
    }
    private void AddMessageToSend(MessageToIpPort message)
    {
        m_toSendPackageToTarget.Enqueue(message); 
        m_messageInQueue = m_toSendPackageToTarget.Count;
    }
    public void TryToSend(string target, string value)
    {
        target = target.Trim();
        

        IpSingleAlias alias = null;
        AliasToGroupOfAlias groupOfAlias = null;
        if (GetGroupOfAlias(target, out groupOfAlias)) {

           AddMessageToSend(new MessageToGroupOfAlias() { m_message = value, m_targets = groupOfAlias.m_targetIdAsString });
        }
        else if (GetAlias(target,out alias)) {


          //  Debug.Log("D:" + target + " V:" + value + "<>");
        }
        else if ( Regex.IsMatch(target, "\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}:\\d{3,3}\\d+"))
        {
            string[] tokenAdd = target.Split(':');
            int port = 0;
            if (int.TryParse(tokenAdd[1], out port)) {
                alias = new IpSingleAlias(target, tokenAdd[0], port);
                AddAlias(alias);
            }
        }
        //Debug.Log("O:" + target + " V:" + value + "<>");

        if (alias!=null)
            AddMessageToSend(new MessageToIpPort(value, ref alias.m_ref));
        
    }

    public  bool GetGroupOfAlias(string target, out AliasToGroupOfAlias groupOfAlias)
    {
        groupOfAlias = null;
        for (int i = 0; i < m_groupAlias.Count; i++)
        {
            if (m_groupAlias[i].m_alias == target) {
                groupOfAlias = m_groupAlias[i];
                return true;
            }

        }
        return false;
    }

    public void ResetTheTargetFromAlias() {

        for (int i = 0; i < m_singleAlias.Count; i++)
        {

            m_targetRegistered.Add(new TargetIpPort(m_singleAlias[i].m_ref.m_ip
               , m_singleAlias[i].m_ref.m_port));
        }
    }

    

    private bool GetAlias(string target, out IpSingleAlias alias)
    {
        alias = null;
        for (int i = 0; i < m_singleAlias.Count; i++)
        {
            if (m_singleAlias[i].m_aliasNameId.Trim().ToLower() == target.Trim().ToLower())
            { 
                alias= m_singleAlias[i];
                return true;
            }
        }
        return false;

    }

    public void AddAlias(string alias, string ip, string port)
    {
        int p;
        if (int.TryParse(port, out p))
        { AddAlias(alias, ip, p); }
    }
    public void AddAlias(string alias, string ip, int port)
    {
        m_singleAlias.Add(new IpSingleAlias(alias, ip, port));
    }
    public void AddAlias(IpSingleAlias alias)
    {
        m_singleAlias.Add(alias);
    }
    public void AddGroupOfAlias(string alias, params string [] groupOfAlias)
    {
        m_groupAlias.Add(new AliasToGroupOfAlias(alias, groupOfAlias));

    }

    public void AddTargetFromTextIPV4(string text) { 
    
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            AddTargetFromLineIPV4(lines[i]);
        }
    }

    public void AddTargetFromLineIPV4(string line) { 
    
        string[] token = line.Split(':');
        if (token.Length == 2)
        {
            int port = 0;
            if (int.TryParse(token[1].Trim(), out port))
            {
                m_targetRegistered.Add(new TargetIpPort(token[0].Trim(), port));
            }
        }
        if (token.Length == 3)
        {
            string name = token[0].Trim();
            string ip = token[1].Trim();
            string port = token[2].Trim();
            AddAlias(name, ip, port);
        }

    }



    private void OnDestroy()
    {
    }


    public void SendAllAsSoonAsPossible() {

        MessageToAll message = null;
        do
        {
            if (m_toSendPackageToAll.Count > 0) { 
                message = m_toSendPackageToAll.Dequeue();
                if (message != null && message.m_message.Trim().Length>0)
                {
                   // Debug.Log("SM:"+message.m_message);
                    for (int i = 0; i < m_targetRegistered.Count; i++)
                    {
                     //   Debug.Log("SMD:" + message.m_message + " - " + m_targetRegistered[i].m_ip);
                        SendMessageTo(m_targetRegistered[i], message.m_message);
                    }
                    //for (int i = 0; i < m_singleAlias.Count; i++)
                    //{
                    //    Debug.Log("SMDA:" + message.m_message + " - " + m_singleAlias[i].m_ref.m_ip);
                    //    SendMessageTo(m_singleAlias[i].m_ref, message.m_message);
                    //}

                }
            }
        } while (m_toSendPackageToAll.Count > 0);

        MessageToIpPort targetIp = null;
        do
        {
            if (m_toSendPackageToTarget.Count > 0)
            {
                targetIp = m_toSendPackageToTarget.Dequeue();
                if (targetIp != null && targetIp.m_message.Trim().Length > 0)
                {

                    SendMessageTo(targetIp.m_ipPort, targetIp.m_message);


                    //Debug.Log("O:" + targetIp.m_ipPort.m_ip+"|"+targetIp.m_ipPort.m_port+ " V:" + targetIp.m_message + "<>");
                }
            }
        } while (m_toSendPackageToTarget.Count > 0);

        MessageToGroupOfAlias aliasMessage = null;
        do
        {
            if (m_toSendPackageWithAlias.Count > 0)
            {
                aliasMessage = m_toSendPackageWithAlias.Dequeue();
                if (aliasMessage != null)
                {

                    List<TargetIpPort> target = GetTarget(aliasMessage);
                    for (int i = 0; i < target.Count; i++)
                    {
                        SendMessageTo(target[i], aliasMessage.m_message);       
                    }
                }
            }
        } while (m_toSendPackageWithAlias.Count > 0);
                            


    }

    private List<TargetIpPort> GetTarget(MessageToGroupOfAlias aliasMessage)
    {
        return GetTarget(aliasMessage.m_targets);
    }
    private List<TargetIpPort> GetTarget(List<string> alias)
    {
        List<TargetIpPort> target = new List<TargetIpPort>();
        for (int i = 0; i < alias.Count; i++)
        {
            for (int j = 0; j < m_singleAlias.Count; j++)
            {
                if (m_singleAlias[j].m_aliasNameId == alias[i])
                    target.Add(m_singleAlias[j].m_ref);
            }

        }
        return target;
    }

    public static string GetIpId(string ip, int port)
    {
        return GetIpId(ip, port.ToString());
    }
    public static string GetIpId(string ip, string port)
    {
        return ip + '|' + port;
    }
    public void SendMessageTo(TargetIpPort target, string message)
    {
        SendMessageTo(target.m_ip, target.m_port, message);
    }
    public void SendMessageTo(string ip, int port, string message)
    {
        m_lastSent = message;
        UdpClientState client = GetClient(ip, port);
        Byte[] sendBytes = Encoding.UTF8.GetBytes(message);
        try
        {
            client.m_client.Send(sendBytes, sendBytes.Length);
        }
        catch (Exception e)
        {
            m_lastException = e.ToString();
        }

    }

    public void SendBytesTo(TargetIpPort target, byte [] sendBytes)
    {
        SendBytesTo(target.m_ip, target.m_port, sendBytes);
    }
    public void SendBytesTo(string ip, int port, byte[] sendBytes)
    {
        UdpClientState client = GetClient(ip, port);
        try
        {
            client.m_client.Send(sendBytes, sendBytes.Length);
            if (sendBytes.Length <= m_maxByteDebugSize)
                m_lastBytesSend= sendBytes;
        }
        catch (Exception e)
        {
            m_lastException = e.ToString();
        }

    }

    private UdpClientState GetClient(string ip, int port)
    {
        UdpClientState client;
        string ipId = GetIpId(ip, port);
        if (m_connections.ContainsKey(ipId))
        {
            client = m_connections[ipId];
        }
        else
        {
            client = new UdpClientState(ip, port);
            m_connections.Add(ipId, client);

        }

        return client;
    }

    public void SentToTarget( MessageToGroupOfAlias target)
    {
        m_toSendPackageWithAlias.Enqueue(target);

    }
    public void SentToTarget(string message)
    {
        m_toSendPackageToAll.Enqueue(new MessageToAll(message));

    }
}

public class UdpClientState
{
    public UdpClient m_client;

    public UdpClientState(string ip, int port)
    {
        m_client = new UdpClient(ip, port);
    }
}

public class  MessageToIpPort{
    public string m_message;
    public TargetIpPort m_ipPort;

    public MessageToIpPort(string message, ref TargetIpPort ipPort)
    {
        m_message = message;
        m_ipPort = ipPort;
    }
}

[System.Serializable]
public class TargetIpPort {
    public string m_ip;
    public int m_port;

    public TargetIpPort(string ip, int port)
    {
        this.m_ip = ip;
        this.m_port = port;
    }

    public string GetAsId() { return m_ip + "|" + m_port; }
    public IpSingleAlias GetAlias() { return new IpSingleAlias(GetAsId(), this); }
}

[System.Serializable]
public class IpSingleAlias{
    public string m_aliasNameId;
    public TargetIpPort m_ref;
    public IpSingleAlias(string aliasNameId, string ip, int port)
    {
        this.m_aliasNameId = aliasNameId;
        this.m_ref = new TargetIpPort(ip, port);
    }
    public IpSingleAlias(string aliasNameId, TargetIpPort reftarget)
    {
        this.m_aliasNameId = aliasNameId;
        this.m_ref = reftarget;
    }
}

[System.Serializable]
public class AliasToGroupOfAlias {
    public string m_alias;
    public List<string> m_targetIdAsString = new List<string>();

    public AliasToGroupOfAlias(string alias, params string[] groupOfAlias)
    {
        m_alias = alias;
        m_targetIdAsString.AddRange( groupOfAlias);
    }

    public void Set(params string[] focusAlias)
    {
        m_targetIdAsString.Clear();
        m_targetIdAsString.AddRange(focusAlias);
    }
    public void Add(string alias)
    {
        if (!m_targetIdAsString.Contains(alias))
            m_targetIdAsString.Add(alias);
    }
    public void Remove(string alias)
    {
        for (int i = m_targetIdAsString.Count-1; i >=0; i--)
        {
            if(m_targetIdAsString[i]== alias)
                m_targetIdAsString.RemoveAt(i);
        }
    }

    public void Clear()
    {
        m_targetIdAsString.Clear();
    }
}


[System.Serializable]
public class MessageToGroupOfAlias
{
    public string m_message;
    public List<string> m_targets = new List<string>();
}
[System.Serializable]
public class MessageToAll
{
    public string m_message;

    public MessageToAll(string message) 
    {
        this.m_message = message;
    }
}
