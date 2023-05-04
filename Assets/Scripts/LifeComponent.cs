using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeComponent : MonoBehaviour
{
    [SerializeField] float m_damageInvulnerableTime;
    [SerializeField] int m_maxHealth;

    public void SetActive(bool enable)
    {
        m_enable = enable;
    }

    void Start()
    {
        RestoreHealth();
        m_enable = true;
    }

    void FixedUpdate()
    {
        if (!m_enable)
        {
            m_disableTimer -= Time.fixedDeltaTime;
            if (m_disableTimer <= 0f)
            {
                SetActive(true);
            }
        }
    }

    void RestoreHealth()
    {
        m_currentHealth = m_maxHealth;
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

        m_currentHealth -= damage;

        BroadcastMessage("OnDamage", m_damageInvulnerableTime);

        if (m_currentHealth <= 0)
        {
            BroadcastMessage("OnDeath");
        }

        m_enable = false;

        m_disableTimer = m_damageInvulnerableTime;
    }

    float m_disableTimer;

    int m_currentHealth;
    bool m_enable;
}
