using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonComponent : EnemyComponent
{
	[SerializeField] bool m_shouldDespawn;
	[Space]

	[SerializeField] float m_activeDistance;
	[Space]

	[SerializeField] float m_moveSpeed;

	// Start is called before the first frame update
	protected virtual void Start()
	{
		ComponentInit();

		m_player = GameObject.FindGameObjectWithTag("Player");
		Debug.Assert(m_player);

		m_persistentHitboxComponent = GetComponent<PersistentHitboxComponent>();
		m_lifeComponent = GetComponent<LifeComponent>();
		Collider2D collider = GetComponent<Collider2D>();

		Debug.Assert(m_persistentHitboxComponent);
		Debug.Assert(m_lifeComponent);

		m_lifeComponent.SetActive(false);
		m_persistentHitboxComponent.SetActive(false);
		collider.enabled = false;
		m_rbody.bodyType = RigidbodyType2D.Static;

		m_movementState = MovementState.init;
	}

	// Update is called once per frame
	protected virtual void FixedUpdate()
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
		else if(m_shouldDespawn && m_movementState != MovementState.init && distSquared.sqrMagnitude > 4 * m_activeDistance * m_activeDistance)
        {
			Destroy(gameObject);
        }
		else
        {
			Move(Vector2.zero);
        }
	}

	void OnInitState()
	{
		Move(Vector2.zero);

		QueryDirectionToPlayer();

		OnEnterSpawnState();
		m_movementState = MovementState.spawn;
	}

	protected void QueryDirectionToPlayer()
    {
		float xToPlayer = transform.position.x - m_player.transform.position.x;

		if (xToPlayer >= 0)
		{
			UpdateDirection(Direction.left);

		}
		else
		{
			UpdateDirection(Direction.right);
		}
	}

	void OnEnterIdleState()
    {
		m_lifeComponent.SetActive(true);
		m_persistentHitboxComponent.SetActive(true);

		Collider2D collider = GetComponent<Collider2D>();
		collider.enabled = true;
		m_rbody.bodyType = RigidbodyType2D.Dynamic;
	}

	protected virtual void OnIdleState()
	{
		Vector2 movement = m_direction == Direction.right ? Vector2.right : Vector2.left;

		movement = movement * m_moveSpeed;
		movement.y = m_rbody.velocity.y;

		Move(movement);
	}

	protected virtual void OnDeadState()
	{
		Move(new(0f, m_rbody.velocity.y));
	}

	protected void OnEnterSpawnState()
    {
		m_stateTimer = 0.5f;
		m_animator.SetTrigger("OnSpawn");
	}

	void OnSpawnState()
    {
		Move(Vector2.zero);

		m_stateTimer -= Time.fixedDeltaTime;

		if (m_stateTimer <= 0f)
        {
			OnEnterIdleState();
			m_movementState = MovementState.idle;
        }
	}

	void OnDamageKnockbackState()
	{
		Move(Vector2.zero);

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

	protected virtual void OnDeath()
    {
		m_lifeComponent.SetActive(false);
		m_persistentHitboxComponent.SetActive(false);

		m_movementState = MovementState.dead;

		// kinda hacky
		Destroy(m_persistentHitboxComponent);

		Destroy(gameObject, 1f);
		m_animator.SetTrigger("OnDeath");
    }

	protected PersistentHitboxComponent m_persistentHitboxComponent;
	protected LifeComponent m_lifeComponent;


	protected float m_stateTimer;
}
