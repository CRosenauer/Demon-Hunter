using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Trigger2DSubscriber : MonoBehaviour
{
    public delegate void TriggerEvent();
    public event TriggerEvent OnTriggerEnter;
    public event TriggerEvent OnTriggerExit;

    void Start()
    {
        Collider2D collider = GetComponent<Collider2D>();
        Debug.Assert(collider.isTrigger);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(OnTriggerEnter != null)
        {
            OnTriggerEnter();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (OnTriggerExit != null)
        {
            OnTriggerExit();
        }
    }
}
