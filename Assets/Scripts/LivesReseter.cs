using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivesReseter : MonoBehaviour
{
    [SerializeField] LivesChangedEvent m_livesChangedEvent;

    void Awake()
    {
        m_livesChangedEvent.Reset();
    }
}

