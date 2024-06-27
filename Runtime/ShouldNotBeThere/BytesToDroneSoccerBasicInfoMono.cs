<<<<<<< HEAD:Runtime/BytesToDroneSoccerBasicInfoMono.cs
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics.Eventing.Reader;
//using UnityEngine;
//using UnityEngine.Events;
=======
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
>>>>>>> 90d071d3a4eb2d1668702c7e30e20fa33079c5f1:Runtime/ShouldNotBeThere/BytesToDroneSoccerBasicInfoMono.cs

//public class BytesToDroneSoccerBasicInfoMono : MonoBehaviour
//{


//    public byte[] m_lastReceived;



//    public byte m_lastByteId;
//    public string m_dateOflastReceived;
//    public int m_minSize=9;


   


//    [System.Serializable]
//    public class ByteReceived { 
    
//        public byte m_id;
//        public byte[] m_bytes;
//    }


//    public UnityEvent<byte, byte[]> m_onByteArrayReceived;


//    public void ParseByte(byte[] bytes)
//    {
//        if (bytes == null)
//        {
//            return;
//        }
//        if(bytes.Length==0)
//        {
//            return;
//        }


//        m_dateOflastReceived = System.DateTime.Now.ToString();
//        m_lastReceived = bytes;
        
//        byte id = bytes[0];
//        m_lastByteId = id;


//        if (bytes.Length >= m_minSize)
//        {
//            PushValideByte(id, bytes);
//        }
//    }

<<<<<<< HEAD:Runtime/BytesToDroneSoccerBasicInfoMono.cs
//    private void PushValideByte(byte id, byte[] bytes)
//    { 
//        m_onByteArrayReceived.Invoke(id, bytes);
//    }
//}

//[System.Serializable]
//public class DronePositionsFrame
//{
//    public long m_timeServer;
//    public long m_frameId;
//    public List<DroneSoccerPosition> m_drones = new List<DroneSoccerPosition>();
//}
//[System.Serializable]
//public class DroneSoccerPosition
//{

//    public int m_indexId_0_11;
//    public int m_droneId_1_12;
//    public Vector3 m_position;
//    public Quaternion m_rotation;

//}
=======
    private void PushValideByte(byte id, byte[] bytes)
    { 
        m_onByteArrayReceived.Invoke(id, bytes);
    }
}
>>>>>>> 90d071d3a4eb2d1668702c7e30e20fa33079c5f1:Runtime/ShouldNotBeThere/BytesToDroneSoccerBasicInfoMono.cs
