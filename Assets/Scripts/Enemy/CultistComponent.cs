using System.Collections;
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

    new void Awake()
    {
        base.Awake();

        m_stateMachine.AddState(CultistState.init, null, OnInitState, null);
        m_stateMachine.AddState(CultistState.idle, OnEnterIdleState, OnIdleState, OnExitIdleState);
        m_stateMachine.AddState(CultistState.retreat, null, OnRetreatState, null);
        m_stateMachine.AddState(CultistState.jump, OnEnterJumpState, OnJumpState, OnExitJumpState);
        m_stateMachine.AddState(CultistState.attack, OnEnterAttackState, null, OnExitAttackState);
        m_stateMachine.AddState(CultistState.fall, OnEnterFallState, OnFallState, OnExitFallState);
        m_stateMachine.AddState(CultistState.death, OnEnterDeathState, null, null);
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

        m_stateMachine.Start(CultistState.init);
    }

    protected override void SetPlayer(GameObject player)
    {
        m_player = player;
        m_playerCollider = m_player.GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        QueryOnGround();

        m_stateMachine.Update();
    }

    void OnInitState()
    {
        m_stateMachine.SetState(CultistState.idle);
    }


    void OnEnterIdleState()
    {
        Move(Vector2.zero);
        QueryDirectionToPlayer();
        Animator.SetTrigger("OnIdle");
        
        if(m_attackTimerCoroutine == null)
        {
            m_attackTimerCoroutine = StartCoroutine(AttackTimerCoroutine());
        }
    }

    void OnIdleState()
    {
        if (!IsOnGround())
        {
            m_stateMachine.SetState(CultistState.fall);
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
            m_stateMachine.SetState(CultistState.retreat);
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

            m_stateMachine.SetState(CultistState.attack);
            return;
        }

        Animator.SetFloat("Speed", m_rbody.velocity.x);
    }

    void OnExitIdleState()
    {
        Animator.ResetTrigger("OnIdle");
    }

    void OnRetreatState()
    {
        if (!IsOnGround())
        {
            m_stateMachine.SetState(CultistState.fall);
            return;
        }

        MoveAwayFromPlayer();

        bool isApproachingWall = QueryRaycastAndStartPoint(
            m_wallCheckStart.transform.position,
            m_wallCheckEnd.transform.position,
            m_physicsLayerMask);

        if (!ShouldRetreat() || isApproachingWall)
        {
            m_stateMachine.SetState(CultistState.idle);
            return;
        }

        bool isApproachingLedge = !QueryStartEndRaycast(m_ledgeCheckStart.transform.position, m_ledgeCheckEnd.transform.position, m_physicsLayerMask);
        if (isApproachingLedge)
        {
            if(IsEscapePlatform())
            {
                m_stateMachine.SetState(CultistState.jump);
                return;
            }
            
            m_stateMachine.SetState(CultistState.idle);
        }
    }


    void OnEnterJumpState()
    {
        JumpAwayFromPlayer();
        Animator.SetTrigger("OnJump");
    }

    void OnJumpState()
    {
        if(IsOnGround())
        { 
            m_stateMachine.SetState(CultistState.idle);
        }
    }

    void OnExitJumpState()
    {
        Animator.ResetTrigger("OnJump");
    }

    void OnEnterAttackState()
    {
        Animator.SetTrigger("OnSpecial");
        StartCoroutine(ExitAttackCoroutine());
    }

    void OnExitAttackState()
    {
        Animator.ResetTrigger("OnSpecial");
    }

    void OnEnterFallState()
    {
        Animator.SetTrigger("OnFall");
        Move(Vector2.zero);
    }

    void OnFallState()
    {
        if(IsOnGround())
        {
            m_stateMachine.SetState(CultistState.idle);
        }
    }

    void OnExitFallState()
    {
        Animator.ResetTrigger("OnFall");
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

        if(m_stateMachine.State == CultistState.attack)
        {
            m_stateMachine.SetState(CultistState.idle);
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
        Animator.SetTrigger("OnDeath");
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
        if(m_stateMachine.State == CultistState.death)
        {
            return;
        }

        m_stateMachine.SetState(CultistState.death);
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

    private FiniteStateMachine<CultistState> m_stateMachine = new();

    Coroutine m_attackTimerCoroutine;

    int m_attackCounter;

    bool m_shouldAttack = false;
}
