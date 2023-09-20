using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// this code should probably bre refactored soon.
// its quickly starting to get unmanageable.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AttackComponent))]
[RequireComponent(typeof(LifeComponent))]
[RequireComponent(typeof(SecondaryWeaponManagerComponent))]
public class PlayerMovement : MovementComponent
{
	[SerializeField] Vector2 m_onHitKnockbackVelocity;

	[SerializeField] float m_moveSpeed;
	[SerializeField] float m_jumpSpeed;

	// Start is called before the first frame update
	void Start()
	{
		ComponentInit();

		m_secondaryWeapon = GetComponent<SecondaryWeaponManagerComponent>();

		m_movementState = MovementState.init;

		_gravity = m_rbody.gravityScale;
	}

	// Update is called once per frame
	void Update()
	{
		if(m_controlable)
        {
			m_userXInput = Input.GetAxisRaw("Horizontal");
			m_userYInput = Input.GetAxisRaw("Vertical");

			UpdateUserInput("Jump", ref m_userJump, ref m_userJumpDownLastFrame);
			UpdateUserInput("Attack", ref m_userAttack, ref m_userAttackDownLastFrame);
			UpdateUserInput("Special", ref m_userSecondaryAttack, ref m_userSecondaryAttackDownLastFrame);
		}
	}

	void UpdateUserInput(string button, ref bool inputFlag, ref bool lastFrameInputFlag)
	{
		bool buttonDown = Input.GetButton(button);

		// only updates the input if it hasnt been consumed
		// represents the button being first down on this frame (seen by the fixed update)
		inputFlag = inputFlag || buttonDown && !lastFrameInputFlag;

		lastFrameInputFlag = buttonDown;
	}

	void OnUnpause()
    {
		// prevents the player from auto-buffering inputs made during pause menu navigation
		m_userAttack = false;
		m_userAttackDownLastFrame = true;
		m_userJump = false;
		m_userJumpDownLastFrame = true;
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
			case MovementState.jump:
				OnJumpState();
				break;
			case MovementState.fall:
				OnFallState();
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
			case MovementState.secondaryWeapon:
				OnSecondaryWeapon();
				break;
		}

		m_userJump = false;
		m_userAttack = false;
	}

	void OnInitState()
    {
		Move(Vector2.zero);
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

		if(m_userSecondaryAttack && CanSecondaryWeapon())
        {
			m_movementState = MovementState.secondaryWeapon;
			OnEnterSecondaryWeapon();
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
			m_movementState = MovementState.jump;
			OnEnterJumpState();
			return;
		}

		TryBufferAttack(m_attackComponent.IsInAttack(), m_userXInput);

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
					Move(moveToStair);

					return;
				}
			}
			
			Vector2 inputMovement = new(m_userXInput * m_moveSpeed, 0f);
			Move(inputMovement);
		}
		else
		{
			Move(Vector2.zero);
		}
	}

	void OnEnterIdleState()
	{
		
	}

	void OnEnterJumpLand()
	{
		m_movementState = MovementState.idle;

		if (IsOnGround())
		{
			m_animator.SetTrigger("OnJumpEnd");
		}

		if(m_attackComponent.IsInAttack())
        {
			if(!m_attackComponent.TryCarryOverAttack(m_movementState))
            {
				m_attackComponent.OnAttackInterrupt();
			}
		}

		ClearAttackBuffer();
		Move(Vector2.zero);

	}

	void OnEnterFallState()
	{
		Move(m_rbody.velocity);

		m_animator.ResetTrigger("OnJumpEnd");
		m_animator.SetTrigger("OnJump");
	}

	void OnEnterFallStateFromIdle()
	{
		m_airTargetVelocity = 0f;
		OnEnterFallState();
	}

	void OnEnterJumpState()
	{
		m_animator.ResetTrigger("OnJumpEnd");
		m_animator.SetTrigger("OnJump");
		m_rbody.gravityScale = _gravity;
		m_airTargetVelocity = m_userXInput * m_moveSpeed;
		Vector2 inputMovement = new(m_airTargetVelocity, m_jumpSpeed);
		Move(inputMovement);
	}

	void AirMove()
	{
		Vector2 velocity = m_rbody.velocity;
		velocity.x = m_airTargetVelocity;
		Move(velocity);
	}

	void OnJumpState()
	{
		// basically lose control over the jump until it's over.
		// may want to add a gravity curve later.

		if (m_rbody.velocity.y < 0f)
		{
			m_movementState = MovementState.fall;
			TryBufferAttack(m_userAttack, m_userXInput);
			return;
		}
		else if (IsOnGround())
		{
			m_movementState = MovementState.idle;
			OnEnterJumpLand();
			return;
		}

		TryBufferAttack(m_userAttack, m_userXInput);

		AirMove();
	}

	void OnEnterDamageKnockbackState()
	{
		Vector2 knockbackVelocity = m_onHitKnockbackVelocity;

		if(GetDirection() == Direction.left)
        {
			knockbackVelocity.x = -knockbackVelocity.x;
		}

		Move(knockbackVelocity);

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
		Move(Vector2.zero);
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
		Move(Vector2.zero);
	}

	void OnExitWalkOnStairToPreJump()
	{
		if (m_stairObject)
		{
			m_stairObject.BroadcastMessage("OnExitStair");
		}

		m_isOnGround = true;
		Move(Vector2.zero);
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
			m_movementState = MovementState.jump;
			UpdateDirect(movement.x);
			OnExitWalkOnStairToPreJump();
			OnEnterJumpState();
			return;
		}

		TryBufferAttack();

		if (!isInAttack)
        {
			UpdateDirect(movement.x);
			Move(movement);
		}
		else
        {
			Move(Vector2.zero);
		}
	}

	void OnEnterSecondaryWeapon()
    {
		return;
    }

	void OnSecondaryWeapon()
    {
		if(ShouldExitSecondaryWeapon())
        {
			ExitSecondaryWeapon();
		}
    }

	bool ShouldExitSecondaryWeapon()
    {
		return true;
    }

	void ExitSecondaryWeapon()
    {
		if(IsOnGround())
        {
			m_movementState = MovementState.idle;
			OnEnterIdleState();
        }
		else
        {
			m_movementState = MovementState.fall;
			OnEnterFallState();
        }
    }

	void CancelSecondaryWeapon()
    {
		return;
    }

	bool CanSecondaryWeapon()
    {
		

		return false;
    }

	void OnFallState()
	{
		if (IsOnGround())
		{
			m_movementState = MovementState.idle;
			OnEnterJumpLand();
			m_animator.ResetTrigger("OnJump");
		}

		TryBufferAttack(m_userAttack, m_userXInput);
		AirMove();
	}

	void OnDamage(float damageInvulnerableTime)
	{
		if(m_movementState == MovementState.walkOnStair)
        {
			OnExitWalkOnStair();
        }

		if(m_movementState == MovementState.secondaryWeapon)
        {
			CancelSecondaryWeapon();
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

	void SetControl(bool control)
    {
		m_controlable = control;

		if(!m_controlable)
        {
			m_userXInput = 0;
			m_userYInput = 0;

			m_userJump = false;
			m_userJumpDownLastFrame = false;

			m_userAttack = false;
			m_userAttackDownLastFrame = false;
		}
	}

	void ForceEmulatedUserInput(Vector2 input)
    {
		m_userXInput = input.x;
		m_userYInput = input.y;
	}

	GameObject m_stairObject;
	StairComponent m_stairComponent;

	SecondaryWeaponManagerComponent m_secondaryWeapon;

	float _gravity;

	float m_userXInput = 0f;
	float m_userYInput = 0f;

	float m_airTargetVelocity;

	bool m_userJump = false;
	bool m_userJumpDownLastFrame = false;

	bool m_userSecondaryAttack = false;
	bool m_userSecondaryAttackDownLastFrame = false;

	bool m_shouldDie = false;
	bool m_controlable = true;
}
