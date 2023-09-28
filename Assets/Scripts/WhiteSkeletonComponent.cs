using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteSkeletonComponent : SkeletonComponent
{
    [SerializeField] GameObject m_ledgeCheckStart;
    [SerializeField] GameObject m_ledgeCheckEnd;
    [Space]

    [SerializeField] float m_respawnTimer;

    protected override void OnIdleState()
    {
        if (IsOnGround() && IsApproachingLedge(m_ledgeCheckStart.transform.position, m_ledgeCheckEnd.transform.position, m_physicsLayerMask))
        {
            UpdateDirection(-Mathf.Sign(transform.localScale.x));
        }

        base.OnIdleState();
    }
}
