using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PersistentHitboxComponent))]
[RequireComponent(typeof(SpriteRenderer))]
public class FanOfKnivesObjectComponent : MovementComponent
{
    [SerializeField] AudioSource m_audioSource;
    [SerializeField] float m_speed;

    // Start is called before the first frame update
    void Start()
    {
        m_movementDirection = transform.rotation * new Vector2(1f, 0f);

        StartCoroutine(DeathTimer());
    }

    void FixedUpdate()
    {
        Move(m_movementDirection * m_speed);

        if(!IsWithinCameraFrustum() && m_destructionCoroutine != null)
        {
            Destroy(gameObject);
        }
    }

    void OnHitOther(int layerMask)
    {
        // hit something but is not touching destructible means we must have touched an enemy
        // er go destroy this!

        int destructibleMask = LayerMask.NameToLayer("Destructible");
        destructibleMask = 1 << destructibleMask;

        if ((layerMask & destructibleMask) == 0)
        {
            m_destructionCoroutine = StartCoroutine(SetupDestroy());
        }
        else
        {
            m_audioSource.Play();
        }
    }

    IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(0.5f);
        if(gameObject)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator SetupDestroy()
    {
        PersistentHitboxComponent hitbox = GetComponent<PersistentHitboxComponent>();
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        hitbox.enabled = false;
        sprite.enabled = false;

        m_audioSource.Play();

        yield return new WaitForSeconds(m_audioSource.clip.length);

        Destroy(gameObject);
    }

    Coroutine m_destructionCoroutine;

    Vector2 m_movementDirection;
}
