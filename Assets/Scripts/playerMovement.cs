using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class playerMovement : MonoBehaviour
{
	[SerializeField] float m_moveSpeed;
	[SerializeField] float m_jumpVelocity;

	enum PlayerMovementState
	{
		idle,
		// walk,
		// preJump,
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
		m_playerMovementState = PlayerMovementState.idle;
	}

	// Update is called once per frame
	void Update()
	{
		m_userXInput = Input.GetAxisRaw("Horizontal");
		m_userJump = Input.GetButton("Jump");
	}

    void FixedUpdate()
    {
		switch(m_playerMovementState)
        {
			case PlayerMovementState.idle:
				OnIdleState();
				break;  
			case PlayerMovementState.jump:
				OnJumpState();
				break;
			case PlayerMovementState.fall:
				OnFallState();
				break;
        }
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
		bool isOnGround = IsOnGround();
		if (m_userJump && isOnGround)
		{
			m_playerMovementState = PlayerMovementState.jump;
			OnEnterJumpState();
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
	}

	void OnJumpState()
	{
		// basically lose control over the jump until it's over.
		// may want to add a gravity curve later.

		if(m_rbody.velocity.y < 0f)
        {
			m_playerMovementState = PlayerMovementState.fall;
        }
		else if(IsOnGround())
        {
			m_playerMovementState = PlayerMovementState.idle;
		}

		AirMove();
	}

	void OnFallState()
	{
		if (IsOnGround())
		{
			m_playerMovementState = PlayerMovementState.idle;
		}

		AirMove();
	}

    void Move(Vector2 direction, float xSpeed)
    {
		Vector2 speedMultiplier = new(xSpeed, 1f);
		Vector2 velocity = direction * speedMultiplier;
		m_rbody.velocity = velocity;
	}

	void AirMove()
    {
		Vector2 velocity = m_rbody.velocity;
		velocity.x = m_carryOverAirSpeed;
		Move(velocity, 1f);
	}

    Rigidbody2D m_rbody;

	float m_userXInput = 0f;
	float m_carryOverAirSpeed = 0f;

	[SerializeField] PlayerMovementState m_playerMovementState;

	bool m_userJump = false;
}
