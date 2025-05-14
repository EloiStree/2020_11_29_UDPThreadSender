using System;
using UnityEngine;
using UnityEngine.Events;

public class UDPIIDMono_ByteToUnityEventWithReceivedDateUTC : MonoBehaviour
{

    public UnityEvent<int, DateTime> m_onIntegerRecevied;
    public UnityEvent<int, int, DateTime> m_onIndexIntegerReceived;
    public UnityEvent<int, int, ulong, DateTime> m_onIndexIntegerDateReceived;
    public UnityEvent<int, ulong, DateTime> m_onIntegerDateReceivedDate;
    public string m_lastReceivedDate = "";
    public ulong m_lastReceivedDateAsLong = 0;
    public void PushIn(byte[] bytes)
    {

        if (bytes.Length == 4)
        {
            int value = System.BitConverter.ToInt32(bytes, 0);
            m_onIntegerRecevied.Invoke(value,DateTime.UtcNow);
            NotifyAsReceived();
        }
        else if (bytes.Length == 8)
        {
            int index = System.BitConverter.ToInt32(bytes, 0);
            int value = System.BitConverter.ToInt32(bytes, 4);
            m_onIndexIntegerReceived.Invoke(index, value, DateTime.UtcNow);
            NotifyAsReceived();
        }
        else if (bytes.Length == 16)
        {
            int index = System.BitConverter.ToInt32(bytes, 0);
            int value = System.BitConverter.ToInt32(bytes, 4);
            ulong date = System.BitConverter.ToUInt64(bytes, 8);
            m_onIndexIntegerDateReceived.Invoke(index, value, date, DateTime.UtcNow);
            NotifyAsReceived();
        }
        else if (bytes.Length == 12)
        {
            int value = System.BitConverter.ToInt32(bytes, 0);
            ulong date = System.BitConverter.ToUInt64(bytes, 4);
            m_onIntegerDateReceivedDate.Invoke(value, date, DateTime.UtcNow);
            NotifyAsReceived();
        }
    }

     ulong GetUlongDateNowUtc()
    {
        return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

    }
     DateTime GetDateNowUtc()
    {

        return DateTime.UtcNow;

    }
     void NotifyAsReceived()
    {
        DateTime date =GetDateNowUtc();
        ulong dateAsLong = GetUlongDateNowUtc();
        m_lastReceivedDate = date.ToString("HH:mm:ss.fffffff") + " UTC";
        m_lastReceivedDateAsLong = dateAsLong;

    }
}
