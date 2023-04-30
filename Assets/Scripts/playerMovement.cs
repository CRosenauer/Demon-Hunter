using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// this code should probably bre refactored soon.
// its quickly starting to get unmanageable.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AttackComponent))]
[RequireComponent(typeof(LifeComponent))]
public class PlayerMovement : MovementComponent
{
	[SerializeField] Vector2 m_onHitKnockbackVelocity;

	[SerializeField] float m_moveSpeed;
	[SerializeField] float m_jumpSpeed;

	// Start is called before the first frame update
	void Start()
	{
		ComponentInit();
		m_lifeComponent = GetComponent<LifeComponent>();

		m_movementState = MovementState.idle;
	}

	// Update is called once per frame
	void Update()
	{
		m_userXInput = Input.GetAxisRaw("Horizontal");

		UpdateUserInput("Jump", ref m_userJump, ref m_userJumpDownLastFrame);
		UpdateUserInput("Attack", ref m_userAttack, ref m_userAttackDownLastFrame);
	}

	void UpdateUserInput(string button, ref bool inputFlag, ref bool lastFrameInputFlag)
	{
		bool attackDown = Input.GetButton(button);
		inputFlag = inputFlag || attackDown && !lastFrameInputFlag;

		lastFrameInputFlag = attackDown;
	}

	void FixedUpdate()
	{
		QueryOnGround();

		switch (m_movementState)
		{
			case MovementState.idle:
				OnIdleState();
				break;
			case MovementState.preJump:
				OnPreJumpState();
				break;
			case MovementState.jump:
				OnJumpState();
				break;
			case MovementState.fall:
				OnFallState();
				break;
			case MovementState.jumpLand:
				OnJumpLandState();
				break;
			case MovementState.damageKnockback:
				OnDamageKnockbackState();
				break;
			case MovementState.dead:
				OnDeadState();
				break;
		}

		m_userJump = false;
		m_userAttack = false;
	}

	// FSM def wont scale well. may need to refactor later.
	void OnIdleState()
	{
		bool isInAttack = m_attackComponent.IsInAttack();

		if (!isInAttack)
		{
			UpdateDirect(m_userXInput);
		}

		bool isOnGround = IsOnGround();

		if (!isOnGround)
		{
			m_movementState = MovementState.fall;
			OnEnterFallStateFromIdle();
			return;
		}
		else if (m_userJump && isOnGround && !isInAttack)
		{
			m_movementState = MovementState.preJump;
			OnEnterPreJumpState();
			return;
		}

		TryBufferAttack();

		if (!isInAttack)
		{
			Vector2 inputMovement = new(m_userXInput, 0f);
			Move(inputMovement, m_moveSpeed);
		}
		else
		{
			Move(Vector2.zero, 0f);
		}
	}

	void OnEnterIdleState()
	{
		// stub
		// may be needed later?
	}

	void OnEnterJumpState()
	{
		m_carryOverAirSpeed = m_moveSpeed * m_preJumpUserInput;
		Vector2 inputMovement = new(m_preJumpUserInput, m_jumpSpeed);
		Move(inputMovement, m_carryOverAirSpeed);
	}

	void OnEnterJumpLand()
	{
		if (IsOnGround())
		{
			m_animator.SetTrigger("OnJumpEnd");
		}

		m_attackComponent.OnAttackInterrupt();
		m_stateTimer = 1f / 6f;
		Move(Vector2.zero, 0f);
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

		m_preJumpUserInput = m_userXInput;

		m_attackBuffered = false;

		// should be in a more visible location. ideally directly tied to the anim length of preJump.
		m_stateTimer = 1f / 6f;

		Move(Vector2.zero, 0f);
	}

	void OnPreJumpState()
	{
		m_stateTimer -= Time.fixedDeltaTime;

		if (m_userAttack)
		{
			m_attackBuffered = true;
		}

		if (m_stateTimer <= 0f)
		{
			m_movementState = MovementState.jump;
			OnEnterJumpState();
		}
	}

	void OnJumpState()
	{
		// basically lose control over the jump until it's over.
		// may want to add a gravity curve later.

		if (m_rbody.velocity.y < 0f)
		{
			m_movementState = MovementState.fall;
			return;
		}
		else if (IsOnGround())
		{
			m_movementState = MovementState.jumpLand;
			OnEnterJumpLand();
			return;
		}

		TryBufferAttack();

		AirMove();
	}

	void OnJumpLandState()
	{
		m_stateTimer -= Time.fixedDeltaTime;

		if (m_stateTimer <= 0f)
		{
			m_movementState = MovementState.idle;
			OnEnterIdleState();
			return;
		}

		m_attackBuffered = m_attackBuffered || m_userAttack;

		m_rbody.velocity = Vector2.zero;
	}

	void OnEnterDamageKnockbackState()
	{
		Vector2 knockbackVelocity = m_onHitKnockbackVelocity;

		if(GetDirection() == Direction.left)
        {
			knockbackVelocity.x = -knockbackVelocity.x;
		}

		Move(knockbackVelocity, 1f);

		m_animator.SetTrigger("OnDamage");
	}

	void OnDamageKnockbackState()
	{
		m_stateTimer -= Time.fixedDeltaTime;

		if (m_stateTimer <= 0f)
		{
			m_animator.ResetTrigger("OnDamage");
			m_movementState = MovementState.idle;
			OnEnterIdleState();
			return;
		}
	}

	void OnEnterDeadState()
	{
		Move(Vector2.zero, 0f);
		m_animator.SetTrigger("OnDeath");
	}

	void OnDeadState()
	{
		// reset at some point?
	}

	void OnFallState()
	{
		if (IsOnGround())
		{
			m_movementState = MovementState.jumpLand;
			OnEnterJumpLand();
			m_animator.ResetTrigger("OnJump");
			m_animator.ResetTrigger("OnFall");
		}

		TryBufferAttack();
		AirMove();
	}

	void OnDamage(float damageInvulnerableTime)
	{
		m_stateTimer = damageInvulnerableTime;

		OnEnterDamageKnockbackState();
		m_movementState = MovementState.damageKnockback;
	}

	void OnDeath()
    {
		OnEnterDeadState();
		m_movementState = MovementState.dead;
	}

	LifeComponent m_lifeComponent;

	float m_userXInput = 0f;
	float m_preJumpUserInput = 0f;

	float m_stateTimer;

	bool m_userJump = false;
	bool m_userJumpDownLastFrame = false;
}
