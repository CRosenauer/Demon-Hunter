using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeToLive : MonoBehaviour
{
    [SerializeField] float m_duration;

    void Start()
    {
        Destroy(gameObject, m_duration);
    }
}
