using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class AttackComponent : MonoBehaviour
{
    [SerializeField] PlayerMovement m_playerMovementComponent;
    [SerializeField] AttackData m_currentAttack;

    [SerializeField] LayerMask m_hitBoxQueryLayer;

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnAttack(PlayerMovement.PlayerMovementState playerMovementState)
    {
        if(m_currentAttack.m_nextAttack != null)
        {
            if(m_frameCount >= m_currentAttack.m_nextAttackWindowStart && m_frameCount < m_currentAttack.m_nextAttackWindowEnd)
            {
                m_currentAttack = m_currentAttack.m_nextAttack;
            }
        }

        // this will easily become unmaintainable.
        // likely some performance or load order gachas i dont know about.
        if(playerMovementState == PlayerMovement.PlayerMovementState.idle)
        {
            m_currentAttack = Resources.Load<AttackData>("AtackData/PlayerGroundAttack1");
        }
        else if (playerMovementState == PlayerMovement.PlayerMovementState.jump)
        {
            m_currentAttack = Resources.Load<AttackData>("AtackData/PlayerAirAttack");
        }

        m_frameCount = 0;

        m_animator.ResetTrigger(oldAnimationTrigger);
        m_animator.SetTrigger(m_currentAttack.m_animationTrigger);
        oldAnimationTrigger = m_currentAttack.m_animationTrigger;
    }

    void OnAttackInterrupt()
    {
        // stub.
        // for the case where the player gets it during an attack, or any other event where the player may end an attack early.
    }

    void FixedUpdate()
    {
        if ( m_frameCount >= m_currentAttack.m_startUpFrames && m_frameCount < (m_currentAttack.m_startUpFrames + m_currentAttack.m_activeFrames) )
        {
            Vector2 playerBasePos = new(transform.position.x, transform.position.y);
            Vector2 hitboxOffset = m_currentAttack.m_collisionOffset;
            Collider2D[] colliders = Physics2D.OverlapBoxAll(playerBasePos + hitboxOffset, m_currentAttack.m_collisionBounds, 0f, m_hitBoxQueryLayer);

            foreach (Collider2D collider in colliders)
            {
                OnHit(collider);
            }
        }

        ++m_frameCount;
    }

    void OnDrawGizmos()
    {
        if(m_currentAttack)
        {
            Gizmos.color = new(1f, 0f, 0f, 0.5f);

            Vector3 hitboxOffset = new(m_currentAttack.m_collisionOffset.x, m_currentAttack.m_collisionOffset.y, 0f);

            if(m_playerMovementComponent.GetDirection() == PlayerMovement.PlayerDirection.left)
            {
                hitboxOffset.x = -hitboxOffset.x;
            }

            Vector3 hitboxBounds = new(m_currentAttack.m_collisionBounds.x, m_currentAttack.m_collisionBounds.y, 1f);
            Gizmos.DrawCube(transform.position + hitboxOffset, hitboxBounds);
        }
    }

    void OnHit(Collider2D collider)
    {

    }

    Animator m_animator;
    string oldAnimationTrigger;

    int m_frameCount = 0;
}
