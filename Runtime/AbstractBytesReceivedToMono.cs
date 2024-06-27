using UnityEngine;

public abstract class AbstractBytesReceivedToMono : MonoBehaviour { 


    public byte[] m_valideId = new byte[] { 1, 2, 3 };
    public byte[] m_lastReceived;


    public void Push(byte id, byte[] bytes)
    {

        if (bytes == null)
        {
            return;
        }
        if (bytes.Length == 0)
        {
            return;
        }
        for (int i = 0; i < m_valideId.Length; i++)
        {
            if (id == m_valideId[i])
            {
                m_lastReceived = bytes;
                PushInParserImplement(id, bytes);
                return;
            }
        }
    }
    protected abstract void PushInParserImplement(byte id, byte[] bytes);
}

