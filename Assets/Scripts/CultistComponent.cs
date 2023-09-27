using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistComponent : EnemyComponent
{
    [SerializeField] GameObject m_attackProjectile;
    [SerializeField] GameObject m_projectileSpawnPoint;
    [Space]

    [SerializeField] GameObject m_wallCheckStart;
    [SerializeField] GameObject m_wallCheckEnd;
    [SerializeField] GameObject m_escapePlatformCheckStart;
    [SerializeField] GameObject m_escapePlatformCheckEnd;
    [SerializeField] GameObject m_ledgeCheckStart;
    [SerializeField] GameObject m_ledgeCheckEnd;
    [Space]

    [SerializeField] GameObject m_playerDetector;
    [Space]

    [SerializeField] Vector2 m_attackDelayRange;
    [Space]

    [SerializeField] float m_moveSpeed;
    [SerializeField] float m_jumpSpeed;
    [Space]

    [SerializeField] LayerMask m_physicsLayerMask;

    enum CultistState
    {
        init,
        idle,
        retreat,
        jump,
        attack,
        death,s
    }

    void Start()
    {
        Debug.Assert(m_attackProjectile);
        Debug.Assert(m_projectileSpawnPoint);
        Debug.Assert(m_wallCheckStart);
        Debug.Assert(m_wallCheckEnd);
        Debug.Assert(m_escapePlatformCheckStart);
        Debug.Assert(m_escapePlatformCheckEnd);
        Debug.Assert(m_ledgeCheckStart);
        Debug.Assert(m_ledgeCheckEnd);
        Debug.Assert(m_playerDetector);

        Trigger2DSubscriber triggerDistributer = m_playerDetector.GetComponent<Trigger2DSubscriber>();
        triggerDistributer.OnTriggerEnter += SetRetreatFlag;
        triggerDistributer.OnTriggerExit += ResetRetreatFlag;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if(player)
        {
            SetPlayer(player);
        }

        QueryDirectionToPlayer();

        m_state = CultistState.init;
        OnEnterInitState();
    }

    void FixedUpdate()
    {
        QueryOnGround();

        switch (m_state)
        {
            case CultistState.init:
                OnInitState();
                break;
            case CultistState.idle:
                OnIdleState();
                break;
            case CultistState.retreat:
                OnRetreatState();
                break;
            case CultistState.jump:
                OnJumpState();
                break;
            case CultistState.attack:
                OnAttackState();
                break;
        }
    }

    void OnEnterInitState()
    {
        
    }

    void OnInitState()
    {
        OnExitInitState();
        OnEnterIdleState();
        m_state = CultistState.idle;
    }

    void OnExitInitState()
    {

    }

    void OnEnterIdleState()
    {
        Move(Vector2.zero);
        QueryDirectionToPlayer();
        m_animator.SetTrigger("OnIdle");
        
        if(m_attackTimerCoroutine == null)
        {
            m_attackTimerCoroutine = StartCoroutine(AttackTimerCoroutine());
        }
    }

    void OnIdleState()
    {
        if (m_shouldRetreat && !IsApprochingWall())
        {
            m_state = CultistState.retreat;
            OnExitIdleState();
            OnEnterRetreatState();
        }

        if (m_shouldAttack)
        {
            m_shouldAttack = false;
            m_attackTimerCoroutine = StartCoroutine(AttackTimerCoroutine());
            m_state = CultistState.attack;
            OnExitIdleState();
            OnEnterAttackState();
            return;
        }

        m_animator.SetFloat("Speed", m_rbody.velocity.x);
    }

    void OnExitIdleState()
    {
        m_animator.ResetTrigger("OnIdle");
    }

    void OnEnterRetreatState()
    {

    }

    void OnRetreatState()
    {
        MoveAwayFromPlayer();

        if (!m_shouldRetreat || IsApprochingWall())
        {
            OnExitRetreatState();
            OnEnterIdleState();
            m_state = CultistState.idle;
            return;
        }

        if(IsApproachingLedge() && IsEscapePlatform())
        {
            OnExitRetreatState();
            OnEnterJumpState();
            m_state = CultistState.jump;
            return;
        }
    }

    void OnExitRetreatState()
    {

    }

    void OnEnterJumpState()
    {
        JumpAwayFromPlayer();
        m_animator.SetTrigger("OnJump");
    }

    void OnJumpState()
    {
        if(IsOnGround())
        {
            OnExitJumpState();
            OnEnterIdleState();
            m_state = CultistState.idle;
        }
    }

    void OnExitJumpState()
    {
        m_animator.ResetTrigger("OnJump");
    }

    void OnEnterAttackState()
    {
        m_animator.SetTrigger("OnSpecial");
        StartCoroutine(ExitAttackCoroutine());
    }

    void OnAttackState()
    {
        
    }

    void OnExitAttackState()
    {
        m_animator.ResetTrigger("OnSpecial");
    }

    void ANIMATION_LaunchAttack()
    {
        GameObject projectile = Instantiate(m_attackProjectile, m_projectileSpawnPoint.transform.position, Quaternion.identity, transform.parent);

        Vector2 targetDirection = GetXDirectionToPlayer(false);
        projectile.SendMessage("OnSetDirection", targetDirection);
    }

    IEnumerator ExitAttackCoroutine()
    {
        yield return new WaitForSeconds(1.25f);

        if(m_state == CultistState.attack)
        {
            OnExitAttackState();
            OnEnterIdleState();
            m_state = CultistState.idle;
        }
    }

    void OnEnterDeathState()
    {
        PersistentHitboxComponent persistentHitboxComponent = GetComponent<PersistentHitboxComponent>();
        if(persistentHitboxComponent)
        {
            persistentHitboxComponent.SetActive(false);
        }

        LifeComponent lifeComponent = GetComponent<LifeComponent>();
        if (lifeComponent)
        {
            lifeComponent.SetActive(false);
        }

        Destroy(persistentHitboxComponent);

        Move(Vector2.zero);

        Destroy(gameObject, 2f);
        m_animator.SetTrigger("OnDeath");
    }

    void OnDeathState()
    {

    }

    void OnExitDeathState()
    {

    }

    bool QueryStartEndRaycast(Vector3 startObjectPosition, Vector3 endObjectPosition)
    {
        Vector2 start = new(startObjectPosition.x, startObjectPosition.y);
        Vector2 end = new(endObjectPosition.x, endObjectPosition.y);

        Vector2 delta = end - start;

        return Physics2D.Raycast(start, delta.normalized, delta.magnitude, m_physicsLayerMask);
    }

    bool IsApproachingLedge()
    {
        return !QueryStartEndRaycast(m_ledgeCheckStart.transform.position, m_ledgeCheckEnd.transform.position);
    }

    bool QueryPointOverlap(Vector3 point)
    {
        Vector2 pointR2 = new(point.x, point.y);
        Collider2D wallOverlapCollider = Physics2D.OverlapPoint(pointR2, m_physicsLayerMask);

        return wallOverlapCollider;
    }

    bool IsApprochingWall()
    {
        Vector3 startPos = m_wallCheckStart.transform.position;
        Vector3 endPos = m_wallCheckEnd.transform.position;

        if(QueryPointOverlap(startPos))
        {
            return true;
        }

        return QueryStartEndRaycast(startPos, endPos);

    }

    bool IsEscapePlatform()
    {
        Vector3 startPos = m_escapePlatformCheckStart.transform.position;
        Vector3 endPos = m_escapePlatformCheckEnd.transform.position;

        // if the starting point in in a wall we assume the ledge is too high
        if (QueryPointOverlap(startPos))
        {
            return false;
        }

        return QueryStartEndRaycast(startPos, endPos);
    }

    void MoveAwayFromPlayer()
    {
        Vector2 movementDirection = GetXDirectionToPlayer(true);

        Move(movementDirection * m_moveSpeed);
        UpdateDirection(movementDirection.x);
    }

    void JumpAwayFromPlayer()
    {
        Vector2 jumpVelocity = GetXDirectionToPlayer(true);
        jumpVelocity = jumpVelocity * m_moveSpeed;
        jumpVelocity.y = m_jumpSpeed;

        Move(jumpVelocity);
    }

    Vector2 GetXDirectionToPlayer(bool away)
    {
        Vector2 playerPosition = m_player.transform.position;
        Vector2 thisPosition = transform.position;
        
        Vector2 deltaPosition = playerPosition - thisPosition;
        deltaPosition.x = Mathf.Sign(deltaPosition.x);
        deltaPosition.y = 0f;

        return away ? -deltaPosition : deltaPosition;
    }

    void SetRetreatFlag()
    {
        m_shouldRetreat = true;
    }

    void ResetRetreatFlag()
    {
         m_shouldRetreat = false;
    }

    // used by life component to signal this has died
    void OnDeath()
    {
        if(m_state == CultistState.death)
        {
            return;
        }

        switch(m_state)
        {
            case CultistState.init:
                OnExitInitState();
                break;
            case CultistState.idle:
                OnExitIdleState();
                break;
            case CultistState.retreat:
                OnExitRetreatState();
                break;
            case CultistState.jump:
                OnExitJumpState();
                break;
        }

        m_state = CultistState.death;
        OnEnterDeathState();
    }

    IEnumerator AttackTimerCoroutine()
    {
        float delayTime = Random.Range(m_attackDelayRange.x, m_attackDelayRange.y);
        yield return new WaitForSeconds(delayTime);

        m_shouldAttack = true;
        m_attackTimerCoroutine = null;
    }

    Coroutine m_attackTimerCoroutine;

    CultistState m_state;

    bool m_shouldAttack = false;
    bool m_shouldRetreat = false;
}
