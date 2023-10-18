using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class TriggerCallback : MonoBehaviour
{
    protected void Start()
    {
        Collider2D collider = GetComponent<Collider2D>();
        Debug.Assert(collider.isTrigger);
    }

    protected abstract void Callback();

    void OnTriggerEnter2D(Collider2D collision)
    {
        Callback();
    }
}
