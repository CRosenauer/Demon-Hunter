using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    [SerializeField] LayerMask m_hitBoxQueryLayer;

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_playerMovementComponent = GetComponent<MovementComponent>();
    }

    void FixedUpdate()
    {
        // a terrible fix
        // for some reason if we reset frame count in another method called from another component's fixed update m_frameCount doesnt get set properly
        if (m_resetFrameCount)
        {
            m_frameCount = 0;
            m_resetFrameCount = false;
        }

        if (IsInActiveWindow())
        {
            Vector2 playerBasePos = new(transform.position.x, transform.position.y);
            Vector2 hitboxOffset = m_currentAttack.m_collisionOffset;
            Collider2D[] colliders = Physics2D.OverlapBoxAll(playerBasePos + hitboxOffset, m_currentAttack.m_collisionBounds, 0f, m_hitBoxQueryLayer);

            foreach (Collider2D collider in colliders)
            {
                OnHit(collider);
            }
        }

        m_frameCount = m_frameCount + 1;
    }

    void OnDrawGizmos()
    {
        if (m_currentAttack)
        {
            if (IsInActiveWindow())
            {
                Gizmos.color = new(1f, 0f, 0f, 0.5f);

                Vector3 hitboxOffset = new(m_currentAttack.m_collisionOffset.x, m_currentAttack.m_collisionOffset.y, 0f);

                if (m_playerMovementComponent.GetDirection() == PlayerMovement.Direction.left)
                {
                    hitboxOffset.x = -hitboxOffset.x;
                }

                Vector3 hitboxBounds = new(m_currentAttack.m_collisionBounds.x, m_currentAttack.m_collisionBounds.y, 1f);
                Gizmos.DrawCube(transform.position + hitboxOffset, hitboxBounds);
            }
        }
    }

    public void OnAttack(PlayerMovement.MovementState playerMovementState)
    {
        bool consecutiveAttack = false;

        if(m_currentAttack != null)
        {
            if(m_frameCount < m_currentAttack.m_interruptableAsSoonAs)
            {
                return;
            }

            if (m_currentAttack.m_nextAttack != null)
            {
                if (IsInAttackDuration())
                {
                    m_currentAttack = m_currentAttack.m_nextAttack;
                    consecutiveAttack = true;
                }
            }
        }
        
        // this will easily become unmaintainable.
        // likely some performance or load order gachas i dont know about.
        if(!consecutiveAttack)
        {
            if (playerMovementState == PlayerMovement.MovementState.idle)
            {
                m_currentAttack = Resources.Load<AttackData>("AttackData/PlayerGroundAttack1");
            }
            else if (playerMovementState == PlayerMovement.MovementState.jump)
            {
                m_currentAttack = Resources.Load<AttackData>("AttackData/PlayerAirAttack");
            }
        }

        Debug.Assert(m_currentAttack);

        m_resetFrameCount = true;

        m_animator.ResetTrigger(oldAnimationTrigger);
        m_animator.SetTrigger(m_currentAttack.m_animationTrigger);
        oldAnimationTrigger = m_currentAttack.m_animationTrigger;
    }

    public bool IsInAttack()
    {
        if(m_currentAttack == null)
        {
            return false;
        }

        if(IsInAttackDuration())
        {
            return true;
        }

        return false;
    }

    public bool CanAttack()
    {
        if(IsInAttack())
        {
            if(m_frameCount >= m_currentAttack.m_interruptableAsSoonAs)
            {
                return true;
            }

            return false;
        }

        return true;
    }

    public void OnAttackInterrupt()
    {
        // stub.
        // for the case where the player gets it during an attack, or any other event where the player may end an attack early.

        m_currentAttack = null;
    }

    bool IsInAttackDuration()
    {
        if(m_currentAttack == null)
        {
            return false;
        }

        return m_frameCount < (m_currentAttack.m_startUpFrames + m_currentAttack.m_activeFrames + m_currentAttack.m_recoveryFrames);
    }

    void OnHit(Collider2D collider)
    {

    }

    bool IsInActiveWindow()
    {
        if(m_currentAttack == null)
        {
            return false;
        }

        return m_frameCount >= m_currentAttack.m_startUpFrames && m_frameCount < (m_currentAttack.m_startUpFrames + m_currentAttack.m_activeFrames);
    }

    MovementComponent m_playerMovementComponent;
    Animator m_animator;

    AttackData m_currentAttack;

    string oldAnimationTrigger;

    int m_frameCount = 0;

    bool m_resetFrameCount = false;
}
