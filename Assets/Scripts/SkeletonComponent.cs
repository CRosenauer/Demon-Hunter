using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonComponent : EnemyComponent
{
	[SerializeField] GameObject m_wallDetectorStart;
	[SerializeField] GameObject m_wallDetectorEnd;

	[SerializeField] float m_activeDistance;
	[Space]

	[SerializeField] float m_moveSpeed;
	[Space]

	[SerializeField] LayerMask m_physicsLayerMask;

	// Start is called before the first frame update
	protected virtual void Start()
	{
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
	protected virtual void FixedUpdate()
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

	void OnInitState()
	{
		Move(Vector2.zero);

		QueryDirectionToPlayer();

		OnEnterSpawnState();
		m_movementState = MovementState.spawn;
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
		if(IsApprochingWall(m_wallDetectorStart.transform.position, m_wallDetectorEnd.transform.position, m_physicsLayerMask))
        {
			UpdateDirection(-Mathf.Sign(transform.localScale.x));
        }

		Vector2 movement = Vector2.right * transform.localScale.x;

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
