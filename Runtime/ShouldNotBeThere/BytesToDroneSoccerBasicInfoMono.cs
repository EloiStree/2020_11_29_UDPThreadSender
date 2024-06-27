using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BytesToDroneSoccerBasicInfoMono : MonoBehaviour
{


    public byte[] m_lastReceived;



    public byte m_lastByteId;
    public string m_dateOflastReceived;
    public int m_minSize=9;


   


    [System.Serializable]
    public class ByteReceived { 
    
        public byte m_id;
        public byte[] m_bytes;
    }


    public UnityEvent<byte, byte[]> m_onByteArrayReceived;


    public void ParseByte(byte[] bytes)
    {
        if (bytes == null)
        {
            return;
        }
        if(bytes.Length==0)
        {
            return;
        }


        m_dateOflastReceived = System.DateTime.Now.ToString();
        m_lastReceived = bytes;
        
        byte id = bytes[0];
        m_lastByteId = id;


        if (bytes.Length >= m_minSize)
        {
            PushValideByte(id, bytes);
        }
    }

    private void PushValideByte(byte id, byte[] bytes)
    { 
        m_onByteArrayReceived.Invoke(id, bytes);
    }
}
