using UnityEngine;

[RequireComponent(typeof(MovementComponent))]
public class PersistentHitboxComponent : MonoBehaviour
{
    [SerializeField] MovementComponent m_movementComponent;
    [Space]

    [SerializeField] LayerMask m_hitBoxQueryLayer;
    [Space]

    [SerializeField] public Vector2 m_collisionBounds;
    [SerializeField] public Vector2 m_collisionOffset;
    [Space]

    [SerializeField] public int m_damage;

    public void SetActive(bool enable, bool indefinite = false)
    {
        m_enable = enable;

        if(indefinite)
        {
            m_disableTimer = float.PositiveInfinity;
        }
    }

    void Start()
    {
        Debug.Assert(m_movementComponent);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!m_enable)
        {
            m_disableTimer -= Time.fixedDeltaTime;
            if(m_disableTimer <= 0f)
            {
                SetActive(true);
            }

            return;
        }

        int touchingLayers = 0;

        Vector2 playerBasePos = new(transform.position.x, transform.position.y);
        Vector2 hitboxOffset = m_collisionOffset;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(playerBasePos + hitboxOffset, m_collisionBounds, 0f, m_hitBoxQueryLayer);

        bool hitSomething = false;

        foreach (Collider2D collider in colliders)
        {
            touchingLayers = touchingLayers | (1 << collider.gameObject.layer);

            LifeComponent lifeComponent = collider.GetComponent<LifeComponent>();

            if(lifeComponent)
            {
                if(lifeComponent.IsInvulnerable())
                {
                     continue;
                }
            }

            hitSomething = true;
            AttackComponent.Hit(collider, m_damage);
        }

        if(hitSomething)
        {
            BroadcastMessage("OnHitOther", touchingLayers);
        }
    }

    void OnDrawGizmos()
    {
        if (m_enable)
        {
            Gizmos.color = new(1f, 0f, 0f, 0.5f);

            Vector3 hitboxOffset = new(m_collisionOffset.x, m_collisionOffset.y, 0f);

            hitboxOffset.x = hitboxOffset.x * transform.localScale.x;

            Vector3 hitboxBounds = new(m_collisionBounds.x, m_collisionBounds.y, 1f);
            Gizmos.DrawCube(transform.position + hitboxOffset, hitboxBounds);
        }
    }

    void OnDamage(float damageInvulnerableTime)
    {
        m_disableTimer = damageInvulnerableTime;

        SetActive(false);
    }


    float m_disableTimer;

    bool m_enable = true;
}
