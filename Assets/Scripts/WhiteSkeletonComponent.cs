using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteSkeletonComponent : SkeletonComponent
{
    [SerializeField] float m_respawnTimer;

    protected override void OnIdleState()
    {
        if(IsOnWall())
        {
            // UpdateDirection(m_direction == Direction.left ? Direction.right : Direction.left);
        }

        base.OnIdleState();
    }

    protected override void OnDeadState()
    {
        base.OnDeadState();

        m_stateTimer -= Time.fixedDeltaTime;

        if(m_stateTimer <= 0f && IsOnGround())
        {
            QueryDirectionToPlayer();

            m_movementState = MovementState.spawn;
            OnEnterSpawnState();

            m_animator.ResetTrigger("OnDeath");
        }
    }

    protected override void OnDeath()
    {
        m_lifeComponent.SetActive(false);
        m_persistentHitboxComponent.SetActive(false, true);

        m_movementState = MovementState.dead;

        m_animator.SetTrigger("OnDeath");

        m_stateTimer = m_respawnTimer;

        Move(new(0f, m_rbody.velocity.y));
    }

    bool IsOnWall()
    {
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        m_rbody.GetContacts(contacts);

        Vector2 compareVector = Vector2.right * transform.localScale.x;

        foreach (ContactPoint2D contact in contacts)
        {
            float dot = Vector2.Dot(contact.normal, compareVector);
            if (dot < -0.99f)
            {
                return true;
            }
        }

        return false;
    }
}
