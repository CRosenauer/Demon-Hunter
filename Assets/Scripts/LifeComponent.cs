using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeComponent : MonoBehaviour
{
    const float hitFlashTime = 0.1f;
    [SerializeField] AudioSource m_hitSoundSource;
    [SerializeField] Material m_hitMaterial;

    [SerializeField] float m_damageInvulnerableTime;
    [SerializeField] int m_maxHealth;
    // [SerializeField] int m_currentHealth;

    SpriteRenderer m_spriteRenderer;
    Material m_defaultSpriteMaterial;

    public delegate void HealthChange(int health);
    public event HealthChange OnHealthChanged;

    public void SetActive(bool enable)
    {
        m_enable = enable;
    }

    public int GetMaxHealth()
    {
        return m_maxHealth;
    }

    public bool IsInvulnerable()
    {
        return m_dead;
    }

    void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        if(m_spriteRenderer)
        {
            m_defaultSpriteMaterial = m_spriteRenderer.material;
        }

        RestoreHealth();
        m_enable = true;
    }

    public bool IsActive()
    {
        return m_enable;
    }

    void FixedUpdate()
    {
        if (!m_enable)
        {
            // should probably be done by a coroutine
            m_disableTimer -= Time.fixedDeltaTime;
            if (m_disableTimer <= 0f)
            {
                SetActive(true);
            }
        }
    }

    void SignalHealthChanged()
    {
        if(OnHealthChanged != null)
        {
            OnHealthChanged(m_currentHealth);
        }
    }

    void RestoreHealth()
    {
        m_currentHealth = m_maxHealth;
        SignalHealthChanged();
    }

    void Heal(int amount)
    {
        m_currentHealth = Mathf.Clamp(m_currentHealth + amount, 0, m_maxHealth);
        SignalHealthChanged();
    }

    void OnStartDisableHurtbox()
    {
        m_enable = false;
        m_disableTimer = m_damageInvulnerableTime;
    }

    void OnDisableHurtbox()
    {
        m_enable = false;
        m_disableTimer = float.PositiveInfinity; // hack
    }

    void OnEnableHurtbox()
    {
        m_enable = true;
        m_disableTimer = 0f;
    }

    void OnHit(int damage)
    {
        if(!m_enable)
        {
            return;
        }

        if(m_currentHealth > 0)
        {
            if (m_spriteRenderer && m_hitMaterial)
            {
                if (m_materialHitReactionCoroutine != null)
                {
                    StopCoroutine(m_materialHitReactionCoroutine);
                }

                m_materialHitReactionCoroutine = StartCoroutine(ActivateHitMaterial(hitFlashTime));
            }

            if(m_hitSoundSource != null)
            {
                m_hitSoundSource.Play();
            }
        }

        m_currentHealth -= damage;
        SignalHealthChanged();

        m_disableTimer = m_damageInvulnerableTime;

        BroadcastMessage("OnDamage", m_damageInvulnerableTime);

        if (m_currentHealth <= 0)
        {
            m_dead = true;
            BroadcastMessage("OnDeath");
        }

        m_enable = false;
    }

    IEnumerator ActivateHitMaterial(float duration)
    {
        m_spriteRenderer.material = m_hitMaterial;
        yield return new WaitForSeconds(duration);
        m_spriteRenderer.material = m_defaultSpriteMaterial;
    }

    IEnumerator ClearFlicker()
    {
        m_spriteRenderer.material = m_hitMaterial;
        yield return new WaitForSecondsRealtime(1f/30f);
        m_spriteRenderer.material = m_defaultSpriteMaterial;
        yield return new WaitForSecondsRealtime(1f / 30f);

        m_clearCoroutine = StartCoroutine(ClearFlicker());
    }

    void OnClear()
    {
        m_clearCoroutine = StartCoroutine(ClearFlicker());
    }

    void OnClearEnd()
    {
        if(m_clearCoroutine != null)
        {
            StopCoroutine(m_clearCoroutine);
            m_spriteRenderer.material = m_defaultSpriteMaterial;
            m_clearCoroutine = null;
        }
    }

    Coroutine m_clearCoroutine;

    Coroutine m_materialHitReactionCoroutine;

    float m_disableTimer;

    int m_currentHealth;
    bool m_enable;

    bool m_dead = false;
}
