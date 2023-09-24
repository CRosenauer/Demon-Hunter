using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyComponent : MovementComponent
{
    void SetPlayer(GameObject player)
    {
        m_player = player;
    }

    protected GameObject m_player;
}
