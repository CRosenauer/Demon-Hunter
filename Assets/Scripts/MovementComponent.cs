using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class MovementComponent : MonoBehaviour
{
    // prob net best ot have all entities draw from this same state pool
    public enum MovementState
    {
        init,
        idle,
        jump,
        fall,
        jumpLand,
        damageKnockback,
        dead,
        deathFall,
        spawn,
        walkOnStair,
        secondaryWeapon,
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

        // doesnt need to exist on the entity
        m_secondaryWeapon = GetComponent<SecondaryWeaponManagerComponent>();

        Debug.Assert(m_rbody);
        Debug.Assert(m_spriteRenderer);
        Debug.Assert(m_animator);
        Debug.Assert(m_attackComponent);
    }

    protected void UpdateDirect(float direction)
    {
        if (Mathf.Approximately(direction, 0f))
        {
            return;
        }

        direction = Mathf.Sign(direction);

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

    protected void UpdateDirection(Direction direction)
    {
        m_direction = direction;

        m_spriteRenderer.flipX = direction == Direction.right ? true: false;
    }

    protected void QueryOnGround()
    {
        const float SPEED_EPSILON = 0.001f;
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

    protected void Move(Vector2 velocity)
    {
        m_rbody.velocity = velocity;

        m_animator.SetFloat("Speed", Mathf.Abs(velocity.x));
    }

    protected void TryBufferAttack(bool updateDirection = false, float direction = 1)
    {
        bool attacked = TryAttack();

        if (!attacked)
        {
            m_attackBuffered = m_attackBuffered || m_userAttack;
        }
        else
        {
            m_attackBuffered = false;

            if (updateDirection)
            {
                UpdateDirect(direction);
            }

        }
    }

    protected bool IsWithinCameraFrustum()
    {
        const float distancePadding = 1f;

        Vector3 screenPos = Camera.current.WorldToScreenPoint(transform.position);

        if (screenPos.x == Mathf.Clamp(screenPos.x, -distancePadding, Screen.width + distancePadding))
        {
            if (screenPos.y == Mathf.Clamp(screenPos.y, -distancePadding, Screen.height + distancePadding))
            {
                return true;
            }
        }

        return false;
    }

    protected bool TryAttack()
    {
        if(m_attackComponent)
        {
            if ((m_attackBuffered || m_userAttack) && m_attackComponent.CanAttack())
            {
                m_attackComponent.OnAttack(m_movementState);
                return true;
            }
        }

        if(m_secondaryWeapon)
        {
            if (m_userSecondaryAttack && m_secondaryWeapon.CanSecondaryAttack())
            {
                m_secondaryWeapon.OnUseSecondaryWeapon();
                return true;
            }
        }

        return false;
    }

    protected void ClearAttackBuffer()
    {
        m_attackBuffered = false;
    }

    public MovementState GetMovmentState()
    {
        return m_movementState;
    }

    [SerializeField] protected MovementState m_movementState;

    protected Rigidbody2D m_rbody;
    SpriteRenderer m_spriteRenderer;
    protected Animator m_animator;
    protected AttackComponent m_attackComponent;
    protected SecondaryWeaponManagerComponent m_secondaryWeapon;

    protected Direction m_direction = Direction.right;

    protected bool m_isOnGround = true;

    protected bool m_userAttack = false;
    protected bool m_userAttackDownLastFrame = false;

    protected bool m_userSecondaryAttack = false;
    protected bool m_userSecondaryAttackDownLastFrame = false;

    protected bool m_attackBuffered = false;
}
