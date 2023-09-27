using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedSkeletonComponent : EnemyComponent
{
	[SerializeField] bool m_shouldDespawn;
	[Space]

	[SerializeField] GameObject m_bone;
	[Space]

	[SerializeField] float m_activeDistance;
	[Space]

	[SerializeField] float m_moveSpeed;
	[SerializeField] float m_shortJumpVelocity;
	[SerializeField] float m_tallJumpVelocity;
	[Space]

	[SerializeField] float m_tallJumpChance;
	[Space]

	[SerializeField] float m_minThrowTime;
	[SerializeField] float m_maxThrowTime;

	// Start is called before the first frame update
	void Start()
    {
		m_player = GameObject.FindGameObjectWithTag("Player");
		Debug.Assert(m_player);

		m_movementState = MovementState.idle;

		m_stateTimer = Random.Range(m_minThrowTime, m_maxThrowTime);
	}

	void FixedUpdate()
	{
		QueryOnGround();

		Vector3 distSquared = transform.position - m_player.transform.position;
		distSquared.z = 0f;

		if (distSquared.sqrMagnitude < m_activeDistance * m_activeDistance || IsWithinCameraFrustum())
		{
			switch (m_movementState)
			{
				case MovementState.idle:
					OnIdleState();
					break;
				case MovementState.dead:
					OnDeadState();
					break;
			}
		}
		else if (m_shouldDespawn && m_movementState != MovementState.init && distSquared.sqrMagnitude > 4 * m_activeDistance * m_activeDistance)
		{
			Destroy(gameObject);
		}
		else
		{
			Move(Vector2.zero);
		}
	}

	void OnIdleState()
    {
		m_stateTimer -= Time.fixedDeltaTime;
		m_boneThrowTimer -= Time.fixedDeltaTime;

		if(m_boneThrowTimer <= 0f)
        {
			ThrowBone();
			m_boneThrowTimer = float.PositiveInfinity; // hack
		}

		if(IsOnGround())
        {
			QueryDirectionToPlayer();

            m_animator.ResetTrigger("OnThrow");
			m_animator.ResetTrigger("OnTallJump");
			m_animator.ResetTrigger("OnShortJump");

			float directionRand = Random.Range(0f, 1f);
			float jumpRand = Random.Range(0f, 1f);

			bool tallJump = jumpRand > m_tallJumpChance;

			float xSpeed = directionRand > 0.5f ? 1f: -1f;
			float ySpeed = tallJump ? m_tallJumpVelocity : m_shortJumpVelocity;

			m_rbody.velocity = new(xSpeed * m_moveSpeed, ySpeed);

			if (m_stateTimer <= 0f)
			{
				m_stateTimer = Random.Range(m_minThrowTime, m_maxThrowTime);

				m_boneThrowTimer = 35f / 60f;
				m_animator.SetTrigger("OnThrow");
				return;
			}

			if(tallJump)
            {
				m_animator.SetTrigger("OnTallJump");
			}
			else
            {
				m_animator.SetTrigger("OnShortJump");
			}
		}
	}

	void OnDeadState()
	{
		Move(new(0f, m_rbody.velocity.y));
	}

	void OnDeath()
	{
		m_animator.SetTrigger("OnDeath");
		m_movementState = MovementState.dead;

		Destroy(GetComponent<PersistentHitboxComponent>());

		Destroy(gameObject, 1f);
	}

	void ThrowBone()
    {
		if (m_bone)
		{
			GameObject spawnedBone = Instantiate(m_bone, transform.position, Quaternion.identity);
			spawnedBone.transform.SetParent(transform.parent);
		}
	}

	float m_stateTimer;
	float m_boneThrowTimer;
}
