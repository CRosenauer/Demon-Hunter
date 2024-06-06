using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class TriggerCallback : MonoBehaviour
{
    [SerializeField] bool m_oneShot;

    protected void Start()
    {
        m_activated = false;
        Collider2D collider = GetComponent<Collider2D>();
        Debug.Assert(collider.isTrigger);
    }

    protected abstract void Callback();

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_oneShot && m_activated)
        {
            return;
        }

        m_activated = true;
        Callback();
    }

    bool m_activated;
}
