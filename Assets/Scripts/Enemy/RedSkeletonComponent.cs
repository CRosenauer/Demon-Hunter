using UnityEngine;

public class RedSkeletonComponent : EnemyComponent
{
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

	public enum RedSkeletonState
    {
		idle,
		dead,
    }

	// Start is called before the first frame update
	new void Start()
    {
		base.Start();

		m_stateMachine.AddState(RedSkeletonState.idle, null, OnIdleState, null);
		m_stateMachine.AddState(RedSkeletonState.dead, null, OnDeadState, null);

		m_stateMachine.Start(RedSkeletonState.idle);

		m_stateTimer = Random.Range(m_minThrowTime, m_maxThrowTime);
	}

	void FixedUpdate()
	{
		m_stateMachine.Update();
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

		if(m_movementComponent.IsOnGround)
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

			m_movementComponent.Move(new Vector2(xSpeed * m_moveSpeed, ySpeed));

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
		m_movementComponent.Move(new(0f, m_movementComponent.Velocity.y));
	}

	void OnDeath()
	{
		ApplyScore();
		m_animator.SetTrigger("OnDeath");
		m_stateMachine.SetState(RedSkeletonState.dead);

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

	FiniteStateMachine<RedSkeletonState> m_stateMachine = new();
}
