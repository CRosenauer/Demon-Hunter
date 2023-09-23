using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaltropObjectComponent : MovementComponent
{
    [SerializeField] AudioSource m_audioSource;
    [SerializeField] float m_speed;
    [SerializeField] float m_deathTimer;

    // Start is called before the first frame update
    void Start()
    {
        ComponentInit();

        m_movementDirection = transform.rotation * new Vector2(1f, 0f);

        Destroy(gameObject, m_deathTimer);

        m_firstUpdate = true;
    }

    void FixedUpdate()
    {
        
        List<ContactPoint2D> contacts = new();
        int contactCount = m_rbody.GetContacts(contacts);
        if (contactCount > 0)
        {
            m_rbody.constraints = RigidbodyConstraints2D.FreezeAll;
            return;
        }

        Vector2 movementVector = m_movementDirection * m_speed;
     
        if(m_firstUpdate)
        {
            m_firstUpdate = false;
        }
        else
        {
            movementVector.y = m_rbody.velocity.y;
        }


        Move(movementVector);
    }

    void OnHitOther()
    {
        m_audioSource.Play();
    }

    Vector2 m_movementDirection;
    bool m_firstUpdate;
}
