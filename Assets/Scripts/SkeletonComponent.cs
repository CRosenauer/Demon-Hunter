using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonComponent : MovementComponent
{
	[SerializeField] float m_activeDistance;
	[Space]

	[SerializeField] float m_moveSpeed;

	// Start is called before the first frame update
	void Start()
	{
		ComponentInit();

		m_player = GameObject.FindGameObjectWithTag("Player");
		Debug.Assert(m_player);

		m_persistentHitboxComponent = GetComponent<PersistentHitboxComponent>();
		m_lifeComponent = GetComponent<LifeComponent>();

		Debug.Assert(m_persistentHitboxComponent);
		Debug.Assert(m_lifeComponent);

		m_lifeComponent.SetActive(false);
		m_persistentHitboxComponent.SetActive(false);

		m_movementState = MovementState.init;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		QueryOnGround();

		Vector3 distSquared = transform.position - m_player.transform.position;

		if(distSquared.sqrMagnitude < m_activeDistance * m_activeDistance)
        {
			switch (m_movementState)
			{
				case MovementState.init:
					OnInitState();
					break;
				case MovementState.idle:
					OnIdleState();
					break;
				case MovementState.damageKnockback:
				 	OnDamageKnockbackState();
				 	break;
				case MovementState.dead:
					OnDeadState();
					break;
				case MovementState.spawn:
					OnSpawnState();
					break;
			}
		}
		else if(distSquared.sqrMagnitude > 4 * m_activeDistance * m_activeDistance)
        {
			Destroy(gameObject);
        }
		else
        {
			Move(Vector2.zero, 0f);
        }
	}

	void OnInitState()
	{
		Move(Vector2.zero, 0f);

		float xToPlayer = transform.position.x - m_player.transform.position.x;

		if(xToPlayer >= 0)
        {
			UpdateDirection(Direction.left);
			
        }
		else
        {
			UpdateDirection(Direction.right);
		}

		OnEnterSpawnState();
		m_movementState = MovementState.spawn;
	}

	void OnEnterIdleState()
    {
		m_lifeComponent.SetActive(true);
		m_persistentHitboxComponent.SetActive(true);
	}

	void OnIdleState()
	{
		Vector2 movement = m_direction == Direction.right ? Vector2.right : Vector2.left;

		movement = movement * m_moveSpeed;

		Move(movement, 1f);
	}

	void OnDeadState()
	{
		Move(Vector2.zero, 1f);
	}

	void OnEnterSpawnState()
    {
		m_stateTimer = 0.5f;
		m_animator.SetTrigger("OnSpawn");
	}

	void OnSpawnState()
    {
		Move(Vector2.zero, 0f);

		m_stateTimer -= Time.fixedDeltaTime;

		if (m_stateTimer <= 0f)
        {
			OnEnterIdleState();
			m_movementState = MovementState.idle;
        }
	}

	void OnDamageKnockbackState()
	{
		Move(Vector2.zero, 0f);

		m_stateTimer -= Time.fixedDeltaTime;

		if(m_stateTimer <= 0f)
        {
			m_movementState = MovementState.idle;
        }
	}

	void OnDamage(float damageInvulnerableTime)
    {
		m_stateTimer = damageInvulnerableTime;
		m_movementState = MovementState.damageKnockback;
	}

	void OnDeath()
    {
		m_lifeComponent.SetActive(false);
		m_persistentHitboxComponent.SetActive(false);

		m_movementState = MovementState.dead;

		// kinda hacky
		Destroy(m_persistentHitboxComponent);

		Destroy(gameObject, 1f);
		m_animator.SetTrigger("OnDeath");
    }

    GameObject m_player;

	PersistentHitboxComponent m_persistentHitboxComponent;
	LifeComponent m_lifeComponent;


	float m_stateTimer;
}
