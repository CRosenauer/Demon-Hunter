using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyComponent : MovementComponent
{
    protected bool QueryStartEndRaycast(Vector2 startObjectPosition, Vector2 endObjectPosition, LayerMask physicsLayerMask)
    {
        Vector2 delta = endObjectPosition - startObjectPosition;

        bool result = Physics2D.Raycast(startObjectPosition, delta.normalized, delta.magnitude, physicsLayerMask);
        return result;
    }

    protected bool IsApproachingLedge(Vector2 startPos, Vector2 endPos, LayerMask physicsLayerMask)
    {
        bool result = !QueryStartEndRaycast(startPos, endPos, physicsLayerMask);
        return result;
    }

    protected bool IsLedgeBehind(Vector2 startPos, Vector2 endPos, LayerMask physicsLayerMask)
    {
        bool result = !QueryStartEndRaycast(startPos, endPos, physicsLayerMask);
        return result;
    }

    protected bool QueryPointOverlap(Vector2 point, LayerMask physicsLayerMask)
    {
        Collider2D wallOverlapCollider = Physics2D.OverlapPoint(point, physicsLayerMask);

        return wallOverlapCollider;
    }

    protected bool IsApprochingWall(Vector2 startPos, Vector2 endPos, LayerMask physicsLayerMask)
    {
        bool result = IsWallBetweenPoints(startPos, endPos, physicsLayerMask);
        return result;
    }

    protected bool IsWallBehind(Vector2 startPos, Vector2 endPos, LayerMask physicsLayerMask)
    {
        bool result = IsWallBetweenPoints(startPos, endPos, physicsLayerMask);
        return result;
    }

    protected bool IsWallBetweenPoints(Vector2 start, Vector2 end, LayerMask physicsLayerMask)
    {
        if (QueryPointOverlap(start, physicsLayerMask))
        {
            return true;
        }

        return QueryStartEndRaycast(start, end, physicsLayerMask);
    }

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
