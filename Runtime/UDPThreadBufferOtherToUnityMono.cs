using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UDPThreadBufferOtherToUnityMono : MonoBehaviour
{

    public Queue<string> m_messageToPushOnMainThread = new Queue<string>();
    public StringUnityEvent m_onMessageRelay;
    [System.Serializable]
    public class StringUnityEvent : UnityEvent<string> { }
 
    void Update()
    {
        if (m_messageToPushOnMainThread.Count > 0) {
            m_onMessageRelay.Invoke(m_messageToPushOnMainThread.Dequeue());
        }
        
    }
    public void EnqueueMessage(string message) {
        m_messageToPushOnMainThread.Enqueue(message);
    }
}
