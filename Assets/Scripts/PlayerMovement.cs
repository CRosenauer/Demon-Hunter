using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(AttackComponent))]
[RequireComponent(typeof(LifeComponent))]
[RequireComponent(typeof(SecondaryWeaponManagerComponent))]
public class PlayerMovement : BaseController
{
	[SerializeField] ScoreChangedEvent m_scoreChangedEvent;

	[SerializeField] Vector2 m_onHitKnockbackVelocity;

	[SerializeField] float m_moveSpeed;
	public float MoveSpeed => m_moveSpeed;

	[SerializeField] float m_jumpSpeed;

	public enum PlayerState
	{
		init,
		idle,
		jump,
		fall,
		jumpLand,
		damageKnockback,
		dead,
		deathFall,
		walkOnStair,
		secondaryWeapon,
	}

	public delegate void PlayerDeath();
	public event PlayerDeath OnPlayerDeath;

    private new void Awake()
    {
		m_attackComponent = GetComponent<AttackComponent>();
		m_secondaryWeapon = GetComponent<SecondaryWeaponManagerComponent>();
		m_playerInput = GetComponent<PlayerInput>();

		base.Awake();

		InitializeStateMachine();
	}

    // Start is called before the first frame update
    void Start()
	{
		m_scoreChangedEvent.Reset();
	}

	private void InitializeStateMachine()
    {
		m_playerStateMachine.AddState(PlayerState.init, null, OnInitState, null);
		m_playerStateMachine.AddState(PlayerState.idle, null, OnIdleState, null);
		m_playerStateMachine.AddState(PlayerState.jump, OnEnterJumpState, OnJumpState, null);
		m_playerStateMachine.AddState(PlayerState.fall, OnEnterFallState, OnFallState, null);
		m_playerStateMachine.AddState(PlayerState.damageKnockback, OnEnterDamageKnockbackState, OnDamageKnockbackState, null);
		m_playerStateMachine.AddState(PlayerState.dead, OnEnterDeadState, null, null);
		m_playerStateMachine.AddState(PlayerState.deathFall, null, OnDeathFallState, null);
		m_playerStateMachine.AddState(PlayerState.walkOnStair, OnEnterWalkOnStair, OnWalkOnStairState, OnExitWalkOnStair);
		m_playerStateMachine.AddState(PlayerState.secondaryWeapon, null, OnSecondaryWeaponState, null);

		m_playerStateMachine.Start(PlayerState.init);
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

		if(m_movementComponent.IsInCutscene)
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

		if(m_movementComponent.IsInCutscene)
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

	public PlayerState GetPlayerState()
	{
		return m_playerStateMachine.State;
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
		m_playerStateMachine.Update();

		ClearUserInput();
	}

	void OnInitState()
    {
		m_movementComponent.Move(Vector2.zero);
		m_playerStateMachine.SetState(PlayerState.idle);
	}

	void OnIdleState()
	{
		if (m_shouldDie)
		{
			m_playerStateMachine.SetState(PlayerState.dead);
			return;
		}

		if(m_userSecondaryAttack && CanSecondaryWeapon())
        {
			m_playerStateMachine.SetState(PlayerState.secondaryWeapon);
        }

		bool isInAttack = m_attackComponent.IsInAttack();
		bool isInSecondaryWeapon = m_secondaryWeapon.IsInSecondaryWeaponAttack();

		bool isAttacking = isInAttack || isInSecondaryWeapon;

		if (!isAttacking)
		{
			m_movementComponent.UpdateDirection(m_userXInput);
		}

		if (!m_movementComponent.IsOnGround)
		{
			m_playerStateMachine.SetState(PlayerState.fall);
			m_animator.SetTrigger("OnJump");

			m_airTargetVelocity = 0f;
			return;
		}
		else if (m_userJump && m_movementComponent.IsOnGround && !isAttacking)
		{
			m_playerStateMachine.SetState(PlayerState.jump);
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
						m_playerStateMachine.SetState(PlayerState.walkOnStair);
						return;
                    }

					m_movementComponent.UpdateDirection(moveToStair.x);
					m_movementComponent.Move(moveToStair);

					return;
				}
			}
			
			Vector2 inputMovement = new(m_userXInput * m_moveSpeed, 0f);
			m_movementComponent.Move(inputMovement);
		}
		else
		{
			m_movementComponent.Move(Vector2.zero);
		}
	}

	void OnJumpLand()
	{
		m_playerStateMachine.SetState(PlayerState.idle);

		if (m_movementComponent.IsOnGround)
		{
			m_movementComponent.Animator.SetTrigger("OnJumpEnd");
		}


		bool isInAttack = m_attackComponent.IsInAttack();

		if (isInAttack)
        {
			if(!m_attackComponent.TryCarryOverAttack(m_playerStateMachine.State))
            {
				m_attackComponent.OnAttackInterrupt();
			}
		}

		if(m_secondaryWeapon.IsInSecondaryWeaponAttack())
        {
			m_secondaryWeapon.CarryOverAttack();
		}

		m_attackBuffered = false;
		m_movementComponent.Move(Vector2.zero);

	}

	void OnEnterFallState()
	{
		m_movementComponent.Move(m_movementComponent.Velocity);

		m_movementComponent.Animator.ResetTrigger("OnJumpEnd");
	}

	void OnEnterJumpState()
	{
		m_movementComponent.Animator.ResetTrigger("OnJumpEnd");
		m_movementComponent.Animator.SetTrigger("OnJump");
		m_movementComponent.ApplyGravity = true;
		m_airTargetVelocity = m_userXInput * m_moveSpeed;
		Vector2 inputMovement = new(m_airTargetVelocity, m_jumpSpeed);
		m_movementComponent.Move(inputMovement);
	}

	void AirMove()
	{
		Vector2 velocity = m_movementComponent.Velocity;
		velocity.x = m_airTargetVelocity;
		m_movementComponent.Move(velocity);
	}

	void OnJumpState()
	{
		// basically lose control over the jump until it's over.
		// may want to add a gravity curve later.

		if (m_movementComponent.Velocity.y < 0f)
		{
			m_playerStateMachine.SetState(PlayerState.fall);
			TryBufferAttack(true, m_userXInput);
			return;
		}
		else if (m_movementComponent.IsOnGround)
		{
			m_playerStateMachine.SetState(PlayerState.idle);
			OnJumpLand();
			return;
		}

		TryBufferAttack(true, m_userXInput);

		AirMove();
	}

	void OnEnterDamageKnockbackState()
	{
		Vector2 knockbackVelocity = m_onHitKnockbackVelocity;

		knockbackVelocity.x = knockbackVelocity.x * transform.localScale.x;

		m_movementComponent.Move(knockbackVelocity);

		m_attackComponent.OnAttackInterrupt();

		m_movementComponent.Animator.SetTrigger("OnDamage");
	}

	void OnDamageKnockbackState()
	{
		if (m_movementComponent.IsOnGround)
		{
			m_movementComponent.Animator.ResetTrigger("OnDamage");

			if (m_shouldDie)
			{
				m_playerStateMachine.SetState(PlayerState.deathFall);
			}
			else
			{
				BroadcastMessage("OnStartDisableHurtbox");
				m_movementComponent.Animator.SetTrigger("OnDamageEnd");
				m_playerStateMachine.SetState(PlayerState.idle);
			}
				
		}
	}

	void OnEnterDeadState()
	{
		m_movementComponent.Move(Vector2.zero);
		m_movementComponent.Animator.SetTrigger("OnDeath");
		BroadcastMessage("OnDisableHurtbox");

		if (OnPlayerDeath != null)
		{
			OnPlayerDeath();
		}
	}

	void OnDeathFallState()
    {
		if (m_movementComponent.IsOnGround)
		{
			m_playerStateMachine.SetState(PlayerState.jumpLand);
			OnJumpLand();
			m_movementComponent.Animator.ResetTrigger("OnJump");
		}

		AirMove();
	}

	void OnEnterWalkOnStair()
    {
		m_movementComponent.ApplyGravity = false;

		m_stairComponent = m_stairObject.GetComponent<StairComponent>();
		if(!m_stairComponent)
        {
			m_playerStateMachine.SetState(PlayerState.idle);
			return;
        }

		m_stairObject.BroadcastMessage("OnEnterStair");
    }

	void OnExitWalkOnStair()
    {
		m_movementComponent.ApplyGravity = true;
		m_movementComponent.Move(Vector2.zero);
	}

	void OnWalkOnStairState()
    {
		if (!m_stairComponent)
		{
			m_playerStateMachine.SetState(PlayerState.idle);

			m_movementComponent.ApplyGravity = true;

			if (m_stairObject)
			{
				m_stairObject.BroadcastMessage("OnExitStair");
			}
			return;
		}

		if(m_stairComponent.ShouldExitStair(transform.position, new(m_userXInput, m_userYInput)))
        {
			transform.position = m_stairComponent.transform.position;

			m_movementComponent.ApplyGravity = true;

			if (m_stairObject)
			{
				m_stairObject.BroadcastMessage("OnExitStair");
			}
			m_playerStateMachine.SetState(PlayerState.idle);
			return;
		}

		Vector2 userInput = new(m_userXInput, m_userYInput);

		Vector2 movement = m_stairComponent.CalculateOnStairMovement(userInput, m_moveSpeed);

		bool isInAttack = m_attackComponent.IsInAttack();
		bool isInSecondaryWeapon = m_secondaryWeapon.IsInSecondaryWeaponAttack();

		bool isAttacking = isInAttack || isInSecondaryWeapon;

		if (m_userJump && !isAttacking)
		{
			m_playerStateMachine.SetState(PlayerState.jump);
			m_movementComponent.UpdateDirection(movement.x);

			if (m_stairObject)
			{
				m_stairObject.BroadcastMessage("OnExitStairJump");
			}
			return;
		}

		TryBufferAttack();

		if (!isAttacking)
        {
			m_movementComponent.UpdateDirection(movement.x);
			m_movementComponent.Move(movement);
		}
		else
        {
			m_movementComponent.Move(Vector2.zero);
		}
	}

	void OnSecondaryWeaponState()
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
		if(m_movementComponent.IsOnGround)
        {
			m_playerStateMachine.SetState(PlayerState.idle);
		}
		else
        {
			m_playerStateMachine.SetState(PlayerState.fall);
			m_movementComponent.Animator.SetTrigger("OnJump");
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

		if (m_movementComponent.IsOnGround)
		{
			m_playerStateMachine.SetState(PlayerState.idle);
			OnJumpLand();
			m_movementComponent.Animator.ResetTrigger("OnJump");
		}

		AirMove();
	}

	void OnDamage(float damageInvulnerableTime)
	{
		if(m_playerStateMachine.State == PlayerState.walkOnStair)
        {
			m_movementComponent.ApplyGravity = true;

			if (m_stairObject)
			{
				m_stairObject.BroadcastMessage("OnExitStairJump");
			}
        }

		if (m_playerStateMachine.State == PlayerState.secondaryWeapon)
        {
			CancelSecondaryWeapon();
        }

		m_playerStateMachine.SetState(PlayerState.damageKnockback);

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
		m_playerStateMachine.SetState(PlayerState.dead);
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
				m_movementComponent.UpdateDirection(direction);
			}

		}
	}

	private bool TryAttack()
	{
		if (m_attackComponent)
		{
			if ((m_attackBuffered || m_userAttack) && m_attackComponent.CanAttack())
			{
				m_attackComponent.OnAttack(m_playerStateMachine.State);
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


	protected AttackComponent m_attackComponent;
	protected SecondaryWeaponManagerComponent m_secondaryWeapon;

	private FiniteStateMachine<PlayerState> m_playerStateMachine = new();

	Vector2 m_cutsceneSavedMovement;

	float _gravity;

	float m_userXInput = 0f;
	float m_userYInput = 0f;

	float m_airTargetVelocity;

	protected bool m_userAttack = false;
	protected bool m_userAttackDownLastFrame = false;

	protected bool m_userSecondaryAttack = false;
	protected bool m_userSecondaryAttackDownLastFrame = false;

	protected bool m_attackBuffered = false;

	bool m_userJump = false;
	bool m_userJumpDownLastFrame = false;

	bool m_shouldDie = false;
	bool m_controlable = true;
}
