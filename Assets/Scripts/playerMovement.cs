using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// this code should probably bre refactored soon.
// its quickly starting to get unmanageable.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AttackComponent))]
public class PlayerMovement : MonoBehaviour
{
	[SerializeField] float m_moveSpeed;
	[SerializeField] float m_jumpVelocity;

	public enum PlayerMovementState
	{
		idle,
		preJump,
		jump,
		fall,
		jumpLand,
		damageKnockback,
	}

	public enum PlayerDirection
    {
		right,
		left
    }

	public PlayerDirection GetDirection()
    {
		return m_direction;
    }

	// Start is called before the first frame update
	void Start()
	{
		m_rbody = GetComponent<Rigidbody2D>();
		m_animator = GetComponent<Animator>();
		m_spriteRenderer = GetComponent<SpriteRenderer>();
		m_attackComponent = GetComponent<AttackComponent>();

		// these asserts arent strictly necessary
		// w/e never hurts
		Debug.Assert(m_rbody);
		Debug.Assert(m_animator);
		Debug.Assert(m_spriteRenderer);
		Debug.Assert(m_attackComponent);

		m_playerMovementState = PlayerMovementState.idle;
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

		switch (m_playerMovementState)
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
			case PlayerMovementState.jumpLand:
				OnJumpLandState();
				break;
		}

		m_userJump = false;
		m_userAttack = false;
	}

	// could generalize later if need be
	bool IsOnGround()
    {
		return m_isOnGround;
    }

	void QueryOnGround()
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

	// FSM def wont scale well. may need to refactor later.
	void OnIdleState()
	{
		bool isInAttack = m_attackComponent.IsInAttack();

		if (!isInAttack)
		{
			if (m_userXInput == 1f)
			{
				m_direction = PlayerDirection.right;
				m_spriteRenderer.flipX = false;
			}
			else if (m_userXInput == -1f)
			{
				m_direction = PlayerDirection.left;
				m_spriteRenderer.flipX = true;
			}
		}

		bool isOnGround = IsOnGround();

		if (!isOnGround)
		{
			m_playerMovementState = PlayerMovementState.fall;
			OnEnterFallStateFromIdle();
			return;
		}
		else if (m_userJump && isOnGround && !isInAttack)
		{
			m_playerMovementState = PlayerMovementState.preJump;
			OnEnterPreJumpState();
			return;
		}

		TryBufferAttack();
		
		if(!isInAttack)
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
		Vector2 inputMovement = new(m_preJumpUserInput, m_jumpVelocity);
		Move(inputMovement, m_carryOverAirSpeed);
	}

	void OnEnterJumpLand()
	{
		if (IsOnGround())
		{
			m_animator.SetTrigger("OnJumpEnd");
		}

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
		m_stateTimer = 1f/6f;

		Move(Vector2.zero, 0f);
	}

	void OnPreJumpState()
	{
		m_stateTimer -= Time.fixedDeltaTime;

		if(m_userAttack)
        {
			m_attackBuffered = true;
		}

		if(m_stateTimer <= 0f)
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
			m_playerMovementState = PlayerMovementState.jumpLand;
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
			m_playerMovementState = PlayerMovementState.idle;
			OnEnterIdleState();
			return;
		}

		m_attackBuffered = m_attackBuffered || m_userAttack;

		m_rbody.velocity = Vector2.zero;
	}

	void OnFallState()
	{
		if (IsOnGround())
		{
			m_playerMovementState = PlayerMovementState.jumpLand;
			OnEnterJumpLand();
			m_animator.ResetTrigger("OnJump");
			m_animator.ResetTrigger("OnFall");
		}

		TryBufferAttack();
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

	void TryBufferAttack()
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

	bool TryAttack()
    {
		if ((m_attackBuffered || m_userAttack) && m_attackComponent.CanAttack())
		{
			m_attackComponent.OnAttack(m_playerMovementState);
			return true;
		}

		return false;
	}

    Rigidbody2D m_rbody;
	Animator m_animator;
	SpriteRenderer m_spriteRenderer;
	AttackComponent m_attackComponent;

	float m_userXInput = 0f;
	float m_carryOverAirSpeed = 0f;
	float m_preJumpUserInput = 0f;

	PlayerDirection m_direction = PlayerDirection.right;

	float m_stateTimer;

	[SerializeField] PlayerMovementState m_playerMovementState;

	bool m_userJump = false;
	bool m_userJumpDownLastFrame = false;

	bool m_userAttack = false;
	bool m_userAttackDownLastFrame = false;

	bool m_attackBuffered = false;

	bool m_isOnGround = true;
}
