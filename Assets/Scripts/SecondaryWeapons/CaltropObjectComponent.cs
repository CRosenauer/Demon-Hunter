using System.Collections.Generic;
using UnityEngine;


public class CaltropObjectComponent : BaseController
{
    [SerializeField] AudioSource m_audioSource;
    [SerializeField] float m_speed;
    [SerializeField] float m_deathTimer;

    void Awake()
    {
        base.Awake();

        Destroy(gameObject, m_deathTimer);

        m_movementComponent.AddOnCollision(OnMovementCollision);

        m_movementComponent.Move(transform.rotation * new Vector2(1f, 0f) * m_speed);
    }

    private void OnDestroy()
    {
        m_movementComponent.RemoveOnCollision(OnMovementCollision);
    }

    void OnHitOther(int layerMask)
    {
        m_audioSource.Play();
    }

    private void OnMovementCollision(Collider2D collider)
    {
        m_movementComponent.Move(Vector2.zero);
    }
}
