using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// this code should probably bre refactored soon.
// its quickly starting to get unmanageable.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
	[SerializeField] float m_moveSpeed;
	[SerializeField] float m_jumpVelocity;

	enum PlayerMovementState
	{
		idle,
		// walk,
		preJump,
		jump,
		fall,
		// jumpLand,
	}

	enum PlayerAttackState
	{
		nonAttack,
		attack,
	}

	// Start is called before the first frame update
	void Start()
	{
		m_rbody = GetComponent<Rigidbody2D>();
		m_animator = GetComponent<Animator>();
		m_spriteRenderer = GetComponent<SpriteRenderer>();

		Debug.Assert(m_rbody);
		Debug.Assert(m_animator);
		Debug.Assert(m_spriteRenderer);

		m_playerMovementState = PlayerMovementState.idle;
	}

	// Update is called once per frame
	void Update()
	{
		m_userXInput = Input.GetAxisRaw("Horizontal");

		bool jumpDown = Input.GetButton("Jump");
		m_userJump = m_userJump || jumpDown && !m_userJumpDownLastFrame;

		m_userJumpDownLastFrame = jumpDown;
	}

    void FixedUpdate()
    {
		switch(m_playerMovementState)
        {
			case PlayerMovementState.idle:
				OnIdleState();
				break;
			case PlayerMovementState.preJump:
				OnPreJumpState();
				break;
			case PlayerMovementState.jump:
				OnJumpState();
				break;
			case PlayerMovementState.fall:
				OnFallState();
				break;
        }

		m_userJump = false;
	}

	// could generalize later if need be
	bool IsOnGround()
    {
		const float SPEED_EPSILON = 0.0001f;
		if(m_rbody.velocity.y > SPEED_EPSILON)
        {
			return false;
        }

		List<ContactPoint2D> contacts = new List<ContactPoint2D>();
		m_rbody.GetContacts(contacts);

		foreach (ContactPoint2D contact in contacts)
        {
			float dot = Vector2.Dot(contact.normal, Vector2.up);
			if(dot >= 0.99f)
            {
				return true;
            }
        }

		return false;
    }

	// FSM def wont scale well. may need to refactor later.
	void OnIdleState()
	{
		if (m_userXInput == 1f)
		{
			m_spriteRenderer.flipX = false;
		}
		else if (m_userXInput == -1f)
		{
			m_spriteRenderer.flipX = true;
		}

		bool isOnGround = IsOnGround();
		if (m_userJump && isOnGround)
		{
			m_playerMovementState = PlayerMovementState.preJump;
			OnEnterPreJumpState();
			return;
		}
		else if(!isOnGround)
        {
			m_playerMovementState = PlayerMovementState.fall;
			OnEnterFallStateFromIdle();
			return;
		}

		Vector2 inputMovement = new(m_userXInput, 0f);
		Move(inputMovement, m_moveSpeed);
	}

	void OnEnterIdleState()
    {
		m_animator.SetTrigger("OnJumpEnd");
	}

	void OnEnterJumpState()
    {
		m_carryOverAirSpeed = m_moveSpeed * m_userXInput;
		Vector2 inputMovement = new(m_userXInput, m_jumpVelocity);
		Move(inputMovement, m_carryOverAirSpeed);
	}

	void OnEnterFallStateFromIdle()
	{
		m_carryOverAirSpeed = 0f;
		Move(m_rbody.velocity, m_carryOverAirSpeed);

		m_animator.ResetTrigger("OnJumpEnd");
		m_animator.SetTrigger("OnFall");
	}

	void OnEnterPreJumpState()
    {
		m_animator.ResetTrigger("OnJumpEnd");
		m_animator.SetTrigger("OnJump");

		// should be in a more visible location. ideally directly tied to the anim length of preJump.
		m_preJumpTimer = 1f/6f;

		Move(Vector2.zero, 0f);
	}

	void OnPreJumpState()
	{
		m_preJumpTimer -= Time.fixedDeltaTime;

		if(m_preJumpTimer <= 0f)
        {
			m_playerMovementState = PlayerMovementState.jump;
			OnEnterJumpState();
		}
	}

	void OnJumpState()
	{
		// basically lose control over the jump until it's over.
		// may want to add a gravity curve later.

		if(m_rbody.velocity.y < 0f)
        {
			m_playerMovementState = PlayerMovementState.fall;
			return;
        }
		else if(IsOnGround())
        {
			m_playerMovementState = PlayerMovementState.idle;
			OnEnterIdleState();
			return;
		}

		AirMove();
	}

	void OnFallState()
	{
		if (IsOnGround())
		{
			m_playerMovementState = PlayerMovementState.idle;
			OnEnterIdleState();
			m_animator.ResetTrigger("OnJump");
			m_animator.ResetTrigger("OnFall");
		}

		AirMove();
	}

    void Move(Vector2 direction, float xSpeed)
    {
		Vector2 speedMultiplier = new(xSpeed, 1f);
		Vector2 velocity = direction * speedMultiplier;
		m_rbody.velocity = velocity;

		m_animator.SetFloat("Speed", Mathf.Abs(velocity.x));
	}

	void AirMove()
    {
		Vector2 velocity = m_rbody.velocity;
		velocity.x = m_carryOverAirSpeed;
		Move(velocity, 1f);
	}

    Rigidbody2D m_rbody;
	Animator m_animator;
	SpriteRenderer m_spriteRenderer;

	float m_userXInput = 0f;
	float m_carryOverAirSpeed = 0f;
	float m_preJumpTimer;

	[SerializeField] PlayerMovementState m_playerMovementState;

	bool m_userJump = false;
	bool m_userJumpDownLastFrame = false;
}
