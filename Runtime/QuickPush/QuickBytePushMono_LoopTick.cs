using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class QuickBytePushMono_LoopTick :MonoBehaviour
{
    public UnityEvent m_onTick;
    public float m_timeBetweenPush = 1;
    public IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(m_timeBetweenPush);
            m_onTick.Invoke();
        }
    }
    
}