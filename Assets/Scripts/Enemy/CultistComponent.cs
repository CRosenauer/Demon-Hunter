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
    [SerializeField] GameObject m_wallCheckBehindStart;
    [SerializeField] GameObject m_wallCheckBehindEnd;
    [SerializeField] GameObject m_escapePlatformCheckStart;
    [SerializeField] GameObject m_escapePlatformCheckEnd;
    [SerializeField] GameObject m_ledgeCheckStart;
    [SerializeField] GameObject m_ledgeCheckEnd;
    [SerializeField] GameObject m_ledgeBehindCheckStart;
    [SerializeField] GameObject m_ledgeBehindCheckEnd;
    [Space]

    [SerializeField] GameObject m_playerDetector;
    [Space]

    [SerializeField] Vector2 m_attackDelayRange;
    [SerializeField] int m_attackGroupingCount;
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
        death,
        fall,
    }

    new void Start()
    {
        Debug.Assert(m_attackProjectile);
        Debug.Assert(m_projectileSpawnPoint);
        Debug.Assert(m_wallCheckStart);
        Debug.Assert(m_wallCheckEnd);
        Debug.Assert(m_wallCheckBehindStart);
        Debug.Assert(m_wallCheckBehindEnd);
        Debug.Assert(m_escapePlatformCheckStart);
        Debug.Assert(m_escapePlatformCheckEnd);
        Debug.Assert(m_ledgeCheckStart);
        Debug.Assert(m_ledgeCheckEnd);
        Debug.Assert(m_playerDetector);

        m_playerDetectorCollider = m_playerDetector.GetComponent<Collider2D>();

        base.Start();

        m_attackCounter = 0;

        m_state = CultistState.init;
        OnEnterInitState();
    }

    protected override void SetPlayer(GameObject player)
    {
        m_player = player;
        m_playerCollider = m_player.GetComponent<Collider2D>();
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
            case CultistState.fall:
                OnFallState();
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
        if (!IsOnGround())
        {
            OnExitIdleState();
            OnEnterFallState();
            m_state = CultistState.fall;
            return;
        }

        bool isBehindWall = QueryRaycastAndStartPoint(
            m_wallCheckBehindStart.transform.position,
            m_wallCheckBehindEnd.transform.position,
            m_physicsLayerMask);

        bool isBehindLedge = !QueryStartEndRaycast(
            m_ledgeBehindCheckStart.transform.position,
            m_ledgeBehindCheckEnd.transform.position,
            m_physicsLayerMask);

        if (ShouldRetreat()
            && !isBehindWall
            && !isBehindLedge)
        {
            m_state = CultistState.retreat;
            OnExitIdleState();
            OnEnterRetreatState();
            return;
        }

        if (m_shouldAttack)
        {
            m_shouldAttack = false;

            if (m_attackTimerCoroutine != null)
            {
                StopCoroutine(m_attackTimerCoroutine);
            }
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
        if (!IsOnGround())
        {
            OnExitRetreatState();
            OnEnterFallState();
            m_state = CultistState.fall;
            return;
        }

        MoveAwayFromPlayer();

        bool isApproachingWall = QueryRaycastAndStartPoint(
            m_wallCheckStart.transform.position,
            m_wallCheckEnd.transform.position,
            m_physicsLayerMask);

        if (!ShouldRetreat() || isApproachingWall)
        {
            OnExitRetreatState();
            OnEnterIdleState();
            m_state = CultistState.idle;
            return;
        }

        bool isApproachingLedge = !QueryStartEndRaycast(m_ledgeCheckStart.transform.position, m_ledgeCheckEnd.transform.position, m_physicsLayerMask);
        if (isApproachingLedge)
        {
            if(IsEscapePlatform())
            {
                OnExitRetreatState();
                OnEnterJumpState();
                m_state = CultistState.jump;
                return;
            }

            OnExitRetreatState();
            OnEnterIdleState();
            m_state = CultistState.idle;
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

    void OnEnterFallState()
    {
        m_animator.SetTrigger("OnFall");
        Move(Vector2.zero);
    }

    void OnFallState()
    {
        if(IsOnGround())
        {
            OnExitFallState();
            OnEnterIdleState();
            m_state = CultistState.idle;
        }
    }

    void OnExitFallState()
    {
        m_animator.ResetTrigger("OnFall");
    }

    void ANIMATION_LaunchAttack()
    {
        GameObject projectile = Instantiate(m_attackProjectile, m_projectileSpawnPoint.transform.position, Quaternion.identity, transform.parent);

        // shoots in the direction the cultist is facing
        Vector2 targetDirection = new(Mathf.Sign(transform.localScale.x), 0f);
        
        projectile.SendMessage("OnSetDirection", targetDirection);
    }

    IEnumerator ExitAttackCoroutine()
    {
        yield return new WaitForSeconds(1.0f);

        if(m_state == CultistState.attack)
        {
            OnExitAttackState();
            OnEnterIdleState();
            m_state = CultistState.idle;
        }
    }

    void OnEnterDeathState()
    {
        ApplyScore();
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

    bool IsEscapePlatform()
    {
        Vector3 startPos = m_escapePlatformCheckStart.transform.position;
        Vector3 endPos = m_escapePlatformCheckEnd.transform.position;

        // if the starting point in in a wall we assume the ledge is too high
        if (QueryPointOverlap(startPos, m_physicsLayerMask))
        {
            return false;
        }

        return QueryStartEndRaycast(startPos, endPos, m_physicsLayerMask);
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

    bool ShouldRetreat()
    {
        return Physics2D.IsTouching(m_playerDetectorCollider, m_playerCollider);
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
        float delayTime;
        m_attackCounter++;
        if (m_attackCounter >= m_attackGroupingCount)
        {
            delayTime = m_attackDelayRange.y;
            m_attackCounter = 0;
        }
        else
        {
            delayTime = m_attackDelayRange.x;
        }

        yield return new WaitForSeconds(delayTime);

        m_shouldAttack = true;
        m_attackTimerCoroutine = null;
    }

    Collider2D m_playerDetectorCollider;
    Collider2D m_playerCollider;

    Coroutine m_attackTimerCoroutine;

    CultistState m_state;

    int m_attackCounter;

    bool m_shouldAttack = false;
}
