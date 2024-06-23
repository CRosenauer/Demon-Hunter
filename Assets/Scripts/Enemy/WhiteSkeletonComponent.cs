using UnityEngine;

public class WhiteSkeletonComponent : SkeletonComponent
{
    [SerializeField] GameObject m_ledgeCheckStart;
    [SerializeField] GameObject m_ledgeCheckEnd;
    [Space]

    [SerializeField] float m_respawnTimer;

    protected override void OnIdleState()
    {
        bool isApproachingLedge = !QueryStartEndRaycast(m_ledgeCheckStart.transform.position, m_ledgeCheckEnd.transform.position, m_physicsLayerMask);
        if (IsOnGround() && isApproachingLedge)
        {
            UpdateDirection(-Mathf.Sign(transform.localScale.x));
        }

        base.OnIdleState();
    }
}
