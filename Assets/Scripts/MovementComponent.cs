using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class MovementComponent : MonoBehaviour
{
    public Animator Animator => m_animator;

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

    protected void Awake()
    {
        m_rbody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
    }

    protected void UpdateDirection(float direction)
    {
        if (Mathf.Approximately(direction, 0f))
        {
            return;
        }

        direction = Mathf.Sign(direction);

        Vector3 scale = transform.localScale;
        scale.x = direction;
        transform.localScale = scale;
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

    public void Move(Vector2 velocity)
    {
        m_rbody.velocity = velocity;

        if(!m_isInCutscene)
        {
            Animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        }
    }

    public void FreezeMovement()
    {
        if(!m_rbody)
        {
            m_rbody = GetComponent<Rigidbody2D>();
        }

        m_rbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void UnfreezeMovement()
    {
        m_rbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    protected void ClearAttackBuffer()
    {
        m_attackBuffered = false;
    }

    public MovementState GetMovmentState()
    {
        return m_movementState;
    }

    public virtual void SetCutscene(bool cutscene)
    {
        m_isInCutscene = cutscene;
    }

    [SerializeField] protected MovementState m_movementState;

    protected Rigidbody2D m_rbody;
    private Animator m_animator;

    protected bool m_isOnGround = true;

    protected bool m_userAttack = false;
    protected bool m_userAttackDownLastFrame = false;

    protected bool m_userSecondaryAttack = false;
    protected bool m_userSecondaryAttackDownLastFrame = false;

    protected bool m_attackBuffered = false;

    protected bool m_isInCutscene = false;
}
