using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondaryWeaponManagerComponent : MonoBehaviour
{
    [SerializeField] int m_maxMana;

    public delegate void ManaChange(int health);
    public event ManaChange OnManaChanged;

    public delegate void WeaponChange(Image weaponImage);
    public event WeaponChange OnWeaponChanged;

    // Start is called before the first frame update
    void Start()
    {
        RestoreMana();
    }

    void SetSecondaryWeapon(SeconaryWeapon weapon)
    {
        m_seconaryWeapon = weapon;

        Image image = m_seconaryWeapon ? m_seconaryWeapon.m_HUDIcon : null;

        OnWeaponChanged(image);
    }

    void OnUseSecondaryWeapon()
    {
        if(!m_seconaryWeapon)
        {
            return;
        }


    }

    void OnSpawnWeapon()
    {
        if (!m_seconaryWeapon)
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

    SeconaryWeapon m_seconaryWeapon;

    int m_currentMana;
}
