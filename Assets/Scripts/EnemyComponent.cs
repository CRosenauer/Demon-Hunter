using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyComponent : MovementComponent
{
    protected void SetPlayer(GameObject player)
    {
        m_player = player;
	}

	protected void QueryDirectionToPlayer()
	{
		float xToPlayer = m_player.transform.position.x - transform.position.x;

		UpdateDirection(xToPlayer);
	}

	protected GameObject m_player;
}
