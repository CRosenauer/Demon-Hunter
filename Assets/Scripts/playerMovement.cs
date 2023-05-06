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

		m_movementState = MovementState.init;

		_gravity = m_rbody.gravityScale;
	}

	// Update is called once per frame
	void Update()
	{
		m_userXInput = Input.GetAxisRaw("Horizontal");
		m_userYInput = Input.GetAxisRaw("Vertical");

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
			case MovementState.init:
				OnInitState();
				break;
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
			case MovementState.deathFall:
				OnDeathFallState();
				break;
			case MovementState.walkOnStair:
				OnWalkOnStair();
				break;
		}

		m_userJump = false;
		m_userAttack = false;
	}

	void OnInitState()
    {
		Move(Vector2.zero, 0f);
		m_movementState = MovementState.idle;
		OnEnterIdleState();
	}

	// FSM def wont scale well. may need to refactor later.
	void OnIdleState()
	{
		if (m_shouldDie)
		{
			OnEnterDeadState();
			m_movementState = MovementState.dead;
			return;
		}

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
			if(m_stairObject && m_stairComponent)
            {
				if(m_stairComponent.CanWalkToStair() && !Mathf.Approximately(m_userYInput, 0f))
                {
					Vector2 inputVector = new(m_userXInput, m_userYInput);
					Vector2 moveToStair = m_stairComponent.CalculateToStairMovement(transform.position, inputVector, m_moveSpeed);

					if (m_stairComponent.CanEnterStair(transform.position) && m_stairComponent.IsInputToEnterStair(inputVector))
                    {
						m_movementState = MovementState.walkOnStair;
						OnEnterWalkOnStair();
						return;
                    }

					UpdateDirect(moveToStair.x);
					Move(moveToStair, 1f);

					return;
				}
			}
			
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
		m_rbody.gravityScale = _gravity;
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
		if(m_shouldDie)
        {
			OnEnterDeadState();
			m_movementState = MovementState.dead;
			return;
		}

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

		m_carryOverAirSpeed = knockbackVelocity.x;

		Move(knockbackVelocity, 1f);

		m_animator.SetTrigger("OnDamage");
	}

	void OnDamageKnockbackState()
	{
		if (IsOnGround())
		{
			m_animator.ResetTrigger("OnDamage");

			if (m_shouldDie)
			{
				m_movementState = MovementState.deathFall;
			}
			else
			{
				BroadcastMessage("OnStartDisableHurtbox");
				m_animator.SetTrigger("OnDamageEnd");
				m_movementState = MovementState.idle;
				OnEnterIdleState();
			}
				
		}
	}

	void OnEnterDeadState()
	{
		Move(Vector2.zero, 0f);
		m_animator.SetTrigger("OnDeath");
		BroadcastMessage("OnDisableHurtbox");
	}

	void OnDeadState()
	{
		// hack. prob doesnt matter?
		// m_animator.ResetTrigger("OnDeath");
	}

	void OnDeathFallState()
    {
		if (IsOnGround())
		{
			m_movementState = MovementState.jumpLand;
			OnEnterJumpLand();
			m_animator.ResetTrigger("OnJump");
			m_animator.ResetTrigger("OnFall");
		}

		AirMove();
	}

	void OnEnterWalkOnStair()
    {
		m_rbody.gravityScale = 0f;

		m_stairComponent = m_stairObject.GetComponent<StairComponent>();
		if(!m_stairComponent)
        {
			m_movementState = MovementState.idle;
			return;
        }

		m_stairObject.BroadcastMessage("OnEnterStair");
    }

	void OnExitWalkOnStair()
    {
		m_rbody.gravityScale = _gravity;

		if(m_stairObject)
        {
			m_stairObject.BroadcastMessage("OnExitStair");
		}
		
		m_isOnGround = true;
		Move(Vector2.zero, 0f);
	}

	void OnExitWalkOnStairToPreJump()
	{
		if (m_stairObject)
		{
			m_stairObject.BroadcastMessage("OnExitStair");
		}

		m_isOnGround = true;
		Move(Vector2.zero, 0f);
	}

	void OnWalkOnStair()
    {
		if (!m_stairComponent)
		{
			m_movementState = MovementState.idle;
		}

		if(m_stairComponent.ShouldExitStair(transform.position, new(m_userXInput, m_userYInput)))
        {
			m_rbody.position = m_stairComponent.transform.position;
			OnExitWalkOnStair();
			m_movementState = MovementState.idle;
			return;
		}

		Vector2 userInput = new(m_userXInput, m_userYInput);

		Vector2 movement = m_stairComponent.CalculateOnStairMovement(userInput, m_moveSpeed);

		bool isInAttack = m_attackComponent.IsInAttack();

		if (m_userJump && !isInAttack)
		{
			m_movementState = MovementState.preJump;
			OnExitWalkOnStairToPreJump();
			OnEnterPreJumpState();
			return;
		}

		TryBufferAttack();

		if (!isInAttack)
        {
			UpdateDirect(movement.x);
			Move(movement, 1f);
		}
		else
        {
			Move(Vector2.zero, 0f);
		}
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
		// m_stateTimer = damageInvulnerableTime;

		if(m_movementState == MovementState.walkOnStair)
        {
			OnExitWalkOnStair();
        }

		OnEnterDamageKnockbackState();
		m_movementState = MovementState.damageKnockback;

		BroadcastMessage("OnDisableHurtbox");
	}

	void OnDeath()
    {
		m_shouldDie = true;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		int layer = LayerMask.GetMask("Stair");
		int shiftedObjectLayer = 1 << other.gameObject.layer;
		if ( (shiftedObjectLayer & layer) != 0 )
        {
			m_stairObject = other.gameObject;
			m_stairComponent = m_stairObject.GetComponent<StairComponent>();
		}
	}

	GameObject m_stairObject;
	StairComponent m_stairComponent;

	float _gravity;

	float m_userXInput = 0f;
	float m_userYInput = 0f;
	float m_preJumpUserInput = 0f;

	float m_stateTimer;

	bool m_userJump = false;
	bool m_userJumpDownLastFrame = false;

	bool m_shouldDie = false;
}
