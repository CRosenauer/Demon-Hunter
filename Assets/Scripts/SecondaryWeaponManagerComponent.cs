using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondaryWeaponManagerComponent : MonoBehaviour
{
    [SerializeField] int m_maxMana;

    public delegate void ManaChange(int health);
    public event ManaChange OnManaChanged;

    public delegate void WeaponChange(Sprite weaponImage);
    public event WeaponChange OnWeaponChanged;

    // Start is called before the first frame update
    void Start()
    {
        RestoreMana();
    }

    bool HasWeapon()
    {
        return m_secondaryWeapon != null;
    }

    void SetSecondaryWeapon(SecondaryWeapon weapon)
    {
        m_secondaryWeapon = weapon;

        Sprite sprite = m_secondaryWeapon ? m_secondaryWeapon.m_HUDIcon : null;

        if (OnWeaponChanged != null)
        {
            OnWeaponChanged(sprite);
        } 
    }

    void OnUseSecondaryWeapon()
    {
        if(!m_secondaryWeapon)
        {
            return;
        }


    }

    void OnSpawnWeapon()
    {
        if (!m_secondaryWeapon)
        {
            return;
        }
    }

    void SignalHealthChanged()
    {
        if (OnManaChanged != null)
        {
            OnManaChanged(m_currentMana);
        }
    }

    void AlterMana(int amount)
    {
        m_currentMana += amount;
        m_currentMana = Mathf.Clamp(m_currentMana, 0, m_maxMana);
        SignalHealthChanged();
    }

    void RestoreMana()
    {
        m_currentMana = m_maxMana;
        SignalHealthChanged();
    }

    public int GetMaxMana()
    {
        return m_maxMana;
    }

    SecondaryWeapon m_secondaryWeapon;

    int m_currentMana;
}
