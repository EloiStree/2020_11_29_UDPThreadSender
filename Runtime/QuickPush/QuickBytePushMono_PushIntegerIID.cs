using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuickBytePushMono_PushIntegerIID : MonoBehaviour
{

    public UnityEvent<byte[]> m_randomBytes;

    

    [ContextMenu("PushFourRandomBytes")]
    public void PushFourRandomBytes()
    {
        byte[] bytes = new byte[4];
        new System.Random().NextBytes(bytes);
        m_randomBytes.Invoke(bytes) ;
    }

    [ContextMenu("PushZeroAsBytes")]
    public void PushIntegerInLittleEndian(int value) { 
    
        byte[] bytes = System.BitConverter.GetBytes(value);
        m_randomBytes.Invoke(bytes);
    }
    [ContextMenu("PushRandomInteger")]
    public void PushRandomInteger() {
        int value = new System.Random().Next();
        PushIntegerInLittleEndian(value);
    }
    [ContextMenu("PushZeroAsInteger")]
    public void PushZeroAsInteger() {         
        PushIntegerInLittleEndian(0);
    }

    [ContextMenu("PushJoinAsInteger")]
    public void PushJoinAsInteger() {

        PushIntegerInLittleEndian(123456789);
        PushIntegerInLittleEndian(987654321);
    }

    [ContextMenu("PushMeaningOfLife")]
    public void PushMeaningOfLife() {
        PushIntegerInLittleEndian(42);
    }


    public void PushPercentAsInteger(float percent) {
        PushIntegerInLittleEndian( (int) Mathf.Round(percent * 100f) );
    }
}
