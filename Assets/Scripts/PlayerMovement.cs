using UnityEngine;
using UnityEngine.InputSystem;


// this code should probably bre refactored soon.
// its quickly starting to get unmanageable.
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AttackComponent))]
[RequireComponent(typeof(LifeComponent))]
[RequireComponent(typeof(SecondaryWeaponManagerComponent))]
public class PlayerMovement : MovementComponent
{
	[SerializeField] ScoreChangedEvent m_scoreChangedEvent;

	[SerializeField] Vector2 m_onHitKnockbackVelocity;

	[SerializeField] float m_moveSpeed;
	[SerializeField] float m_jumpSpeed;

	public delegate void PlayerDeath();
	public event PlayerDeath OnPlayerDeath;

	// Start is called before the first frame update
	void Start()
	{
		m_scoreChangedEvent.Reset();

		m_playerInput = GetComponent<PlayerInput>();
		m_movementState = MovementState.init;

		_gravity = m_rbody.gravityScale;
	}

	public void OnMove(InputAction.CallbackContext context)
    {
		Vector2 moveInput = context.ReadValue<Vector2>();

		float inputX = Mathf.Approximately(moveInput.x, 0f) ? 0f : Mathf.Sign(moveInput.x);
		float inputY = Mathf.Approximately(moveInput.y, 0f) ? 0f : Mathf.Sign(moveInput.y);

		if (m_controlable)
        {
			m_userXInput = inputX;
			m_userYInput = inputY;
		}
		else
        {
			m_cutsceneSavedMovement = new(inputX, inputY);
		}

		if(m_isInCutscene)
        {
			m_cutsceneSavedMovement = new(m_userXInput, m_userYInput);
		}
	}

    public void OnTogglePause(InputAction.CallbackContext context)
    {
		if(context.performed)
        {
			GameObject systems = GameObject.Find("Systems");
			systems.SendMessage("TogglePause");
		}
	}

	// Update is called once per frame
	void Update()
	{
		if(m_controlable)
        {
			UpdateUserInput("Jump", ref m_userJump, ref m_userJumpDownLastFrame);
			UpdateUserInput("Attack", ref m_userAttack, ref m_userAttackDownLastFrame);
			UpdateUserInput("Special", ref m_userSecondaryAttack, ref m_userSecondaryAttackDownLastFrame);
		}

		if(m_isInCutscene)
        {
			m_userXInput = 0f;
			m_userYInput = 0f;

			m_userJump = false;
			m_userAttack = false;
			m_userSecondaryAttack = false;

			m_userJumpDownLastFrame = false;
			m_userAttackDownLastFrame = false;
			m_userSecondaryAttackDownLastFrame = false;
		}
	}

	public override void SetCutscene(bool cutscene)
	{
		m_isInCutscene = cutscene;

		if(m_isInCutscene)
        {
			m_cutsceneSavedMovement = new(m_userXInput, m_userYInput);
		}
		else
        {
			m_userXInput = m_cutsceneSavedMovement.x;
			m_userYInput = m_cutsceneSavedMovement.y;

		}
	}

	void UpdateUserInput(string button, ref bool inputFlag, ref bool lastFrameInputFlag)
	{
		bool buttonDown = m_playerInput.actions[button].WasPerformedThisFrame();

		// only updates the input if it hasnt been consumed
		// represents the button being first down on this frame (seen by the fixed update)
		inputFlag = inputFlag || buttonDown && !lastFrameInputFlag;

		lastFrameInputFlag = buttonDown;
	}

	void ClearUserInput()
    {
		m_userJump = false;
		m_userAttack = false;
		m_userSecondaryAttack = false;
	}

	void OnUnpause()
    {
		// prevents the player from auto-buffering inputs made during pause menu navigation
		m_userAttack = false;
		m_userAttackDownLastFrame = true;
		m_userJump = false;
		m_userJumpDownLastFrame = true;
		m_userSecondaryAttack = false;
		m_userSecondaryAttackDownLastFrame = true;
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

		ClearUserInput();
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
		bool isInSecondaryWeapon = m_secondaryWeapon.IsInSecondaryWeaponAttack();

		bool isAttacking = isInAttack || isInSecondaryWeapon;

		if (!isAttacking)
		{
			UpdateDirection(m_userXInput);
		}

		bool isOnGround = IsOnGround();

		if (!isOnGround)
		{
			m_movementState = MovementState.fall;
			OnEnterFallStateFromIdle();
			return;
		}
		else if (m_userJump && isOnGround && !isAttacking)
		{
			m_movementState = MovementState.jump;
			OnEnterJumpState();
			return;
		}

		TryBufferAttack(true, m_userXInput);

		if (!isAttacking)
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

					UpdateDirection(moveToStair.x);
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


		bool isInAttack = m_attackComponent.IsInAttack();

		if (isInAttack)
        {
			if(!m_attackComponent.TryCarryOverAttack(m_movementState))
            {
				m_attackComponent.OnAttackInterrupt();
			}
		}

		if(m_secondaryWeapon.IsInSecondaryWeaponAttack())
        {
			m_secondaryWeapon.CarryOverAttack();
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
			TryBufferAttack(true, m_userXInput);
			return;
		}
		else if (IsOnGround())
		{
			m_movementState = MovementState.idle;
			OnEnterJumpLand();
			return;
		}

		TryBufferAttack(true, m_userXInput);

		AirMove();
	}

	void OnEnterDamageKnockbackState()
	{
		Vector2 knockbackVelocity = m_onHitKnockbackVelocity;

		knockbackVelocity.x = knockbackVelocity.x * transform.localScale.x;

		Move(knockbackVelocity);

		m_attackComponent.OnAttackInterrupt();

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

		if (OnPlayerDeath != null)
		{
			OnPlayerDeath();
		}
	}

	void OnDeadState()
	{
		// placeholder. could need it later
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

	void OnExitWalkOnStairDamage()
	{
		m_rbody.gravityScale = _gravity;

		if (m_stairObject)
		{
			m_stairObject.BroadcastMessage("OnExitStairJump");
		}

		m_isOnGround = true;
		Move(Vector2.zero);
	}

	void OnExitWalkOnStairToPreJump()
	{
		if (m_stairObject)
		{
			m_stairObject.BroadcastMessage("OnExitStairJump");
		}

		m_isOnGround = true;
		Move(Vector2.zero);
	}

	void OnWalkOnStair()
    {
		if (!m_stairComponent)
		{
			m_movementState = MovementState.idle;
			OnExitWalkOnStair();
			OnEnterIdleState();
			return;
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
		bool isInSecondaryWeapon = m_secondaryWeapon.IsInSecondaryWeaponAttack();

		bool isAttacking = isInAttack || isInSecondaryWeapon;

		if (m_userJump && !isAttacking)
		{
			m_movementState = MovementState.jump;
			UpdateDirection(movement.x);
			OnExitWalkOnStairToPreJump();
			OnEnterJumpState();
			return;
		}

		TryBufferAttack();

		if (!isAttacking)
        {
			UpdateDirection(movement.x);
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
		TryBufferAttack(true, m_userXInput);

		if (IsOnGround())
		{
			m_movementState = MovementState.idle;
			OnEnterJumpLand();
			m_animator.ResetTrigger("OnJump");
		}

		AirMove();
	}

	void OnDamage(float damageInvulnerableTime)
	{
		if(m_movementState == MovementState.walkOnStair)
        {
			OnExitWalkOnStairDamage();
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
			m_cutsceneSavedMovement.x = m_userXInput;
			m_cutsceneSavedMovement.y = m_userYInput;

			m_userXInput = 0;
			m_userYInput = 0;

			m_userJump = false;
			m_userJumpDownLastFrame = false;

			m_userAttack = false;
			m_userAttackDownLastFrame = false;
		}
		else
        {
			m_userXInput = m_cutsceneSavedMovement.x;
			m_userYInput = m_cutsceneSavedMovement.y;
		}
	}

	void ForceEmulatedUserInput(Vector2 input)
    {
		m_userXInput = input.x;
		m_userYInput = input.y;
	}

	void ForceDeath()
    {
		// don't need to worry about proper state exits. object will be reloaded on respawn
		m_movementState = MovementState.dead;
		OnEnterDeadState();
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
				UpdateDirection(direction);
			}

		}
	}

	private bool TryAttack()
	{
		if (m_attackComponent)
		{
			if ((m_attackBuffered || m_userAttack) && m_attackComponent.CanAttack())
			{
				m_attackComponent.OnAttack(m_movementState);
				return true;
			}
		}

		if (m_secondaryWeapon)
		{
			if (m_userSecondaryAttack && m_secondaryWeapon.CanSecondaryAttack())
			{
				m_secondaryWeapon.OnUseSecondaryWeapon();
				return true;
			}
		}

		return false;
	}

	GameObject m_stairObject;
	StairComponent m_stairComponent;

	PlayerInput m_playerInput;

	Vector2 m_cutsceneSavedMovement;

	float _gravity;

	float m_userXInput = 0f;
	float m_userYInput = 0f;

	float m_airTargetVelocity;

	bool m_userJump = false;
	bool m_userJumpDownLastFrame = false;

	bool m_shouldDie = false;
	bool m_controlable = true;
}
