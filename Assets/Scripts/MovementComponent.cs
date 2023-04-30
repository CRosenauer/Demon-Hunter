using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AttackComponent))]
public class MovementComponent : MonoBehaviour
{
    public enum MovementState
    {
        idle,
        preJump,
        jump,
        fall,
        jumpLand,
        damageKnockback,
        dead,
        deathFall
    }

    public enum Direction
    {
        right,
        left
    }

    public Direction GetDirection()
    {
        return m_direction;
    }

    // Start is called before the first frame update
    void Start()
    {
        ComponentInit();
    }

    protected virtual void ComponentInit()
    {
        m_rbody = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
        m_attackComponent = GetComponent<AttackComponent>();

        Debug.Assert(m_rbody);
        Debug.Assert(m_spriteRenderer);
        Debug.Assert(m_animator);
        Debug.Assert(m_attackComponent);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    protected void UpdateDirect(float direction)
    {
        if (direction == 1f)
        {
            m_direction = Direction.right;
            m_spriteRenderer.flipX = false;
        }
        else if (direction == -1f)
        {
            m_direction = Direction.left;
            m_spriteRenderer.flipX = true;
        }
    }

    protected void QueryOnGround()
    {
        const float SPEED_EPSILON = 0.0001f;
        if (m_rbody.velocity.y > SPEED_EPSILON)
        {
            m_isOnGround = false;
            return;
        }

        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        m_rbody.GetContacts(contacts);

        foreach (ContactPoint2D contact in contacts)
        {
            float dot = Vector2.Dot(contact.normal, Vector2.up);
            if (dot >= 0.99f)
            {
                m_isOnGround = true;
                return;
            }
        }

        m_isOnGround = false;
    }

    protected bool IsOnGround()
    {
        return m_isOnGround;
    }

    protected void Move(Vector2 direction, float xSpeed)
    {
        Vector2 speedMultiplier = new(xSpeed, 1f);
        Vector2 velocity = direction * speedMultiplier;
        m_rbody.velocity = velocity;

        m_animator.SetFloat("Speed", Mathf.Abs(velocity.x));
    }

    protected void AirMove()
    {
        Vector2 velocity = m_rbody.velocity;
        velocity.x = m_carryOverAirSpeed;
        Move(velocity, 1f);
    }

    protected void TryBufferAttack()
    {
        if (!TryAttack())
        {
            m_attackBuffered = m_attackBuffered || m_userAttack;
        }
        else
        {
            m_attackBuffered = false;
        }
    }

    protected bool TryAttack()
    {
        if ((m_attackBuffered || m_userAttack) && m_attackComponent.CanAttack())
        {
            m_attackComponent.OnAttack(m_movementState);
            return true;
        }

        return false;
    }

    [SerializeField] protected MovementState m_movementState;

    protected Rigidbody2D m_rbody;
    SpriteRenderer m_spriteRenderer;
    protected Animator m_animator;
    protected AttackComponent m_attackComponent;

    protected float m_carryOverAirSpeed = 0f;

    Direction m_direction = Direction.right;

    bool m_isOnGround = true;

    protected bool m_userAttack = false;
    protected bool m_userAttackDownLastFrame = false;

    protected bool m_attackBuffered = false;
}
