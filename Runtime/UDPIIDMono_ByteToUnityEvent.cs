using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UDPIIDMono_ByteToUnityEvent : MonoBehaviour
{

    public UnityEvent<int> m_onIntegerRecevied;
    public UnityEvent<int, int> m_onIndexIntegerReceived;
    public UnityEvent<int, int, ulong> m_onIndexIntegerDateReceived;
    public UnityEvent<int, ulong> m_onIntegerDateReceivedDate;

    public void PushIn(byte[] bytes)
    {

        if (bytes.Length == 4)
        {
            int value = System.BitConverter.ToInt32(bytes, 0);
            m_onIntegerRecevied.Invoke(value);
        }
        else if (bytes.Length == 8)
        {
            int index = System.BitConverter.ToInt32(bytes, 0);
            int value = System.BitConverter.ToInt32(bytes, 4);
            m_onIndexIntegerReceived.Invoke(index, value);
        }
        else if (bytes.Length == 16)
        {
            int index = System.BitConverter.ToInt32(bytes, 0);
            int value = System.BitConverter.ToInt32(bytes, 4);
            ulong date = System.BitConverter.ToUInt64(bytes, 8);
            m_onIndexIntegerDateReceived.Invoke(index, value, date);
        }
        else if (bytes.Length == 12)
        {
            int value = System.BitConverter.ToInt32(bytes, 0);
            ulong date = System.BitConverter.ToUInt64(bytes, 4);
            m_onIntegerDateReceivedDate.Invoke(value, date);
        }
    }


}
