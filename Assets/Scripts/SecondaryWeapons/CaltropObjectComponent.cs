using System.Collections.Generic;
using UnityEngine;


public class CaltropObjectComponent : BaseController
{
    [SerializeField] AudioSource m_audioSource;
    [SerializeField] float m_speed;
    [SerializeField] float m_deathTimer;

    void Start()
    {
        m_movementDirection = transform.rotation * new Vector2(1f, 0f);

        Destroy(gameObject, m_deathTimer);

        m_movementComponent.Move(m_movementDirection * m_speed);
    }

    void OnHitOther(int layerMask)
    {
        m_audioSource.Play();
    }

    Vector2 m_movementDirection;
}
