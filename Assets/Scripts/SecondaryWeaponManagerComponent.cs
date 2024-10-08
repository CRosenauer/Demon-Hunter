using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SecondaryWeaponManagerComponent : MonoBehaviour
{
    [SerializeField] AudioSource m_secondaryWeaponUsageSound;
    [SerializeField] int m_maxMana;

    public delegate void ManaChange(int health);
    public event ManaChange OnManaChanged;

    public delegate void WeaponChange(Sprite weaponImage);
    public event WeaponChange OnWeaponChanged;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(m_secondaryWeaponUsageSound);

        Animator = GetComponent<Animator>();
        ResetMana();
    }

    public bool IsInSecondaryWeaponAttack()
    {
        if(m_weaponSpawnCoroutine != null)
        {
            return true;
        }

        return false;
    }

    public bool CanSecondaryAttack()
    {
        if(!m_secondaryWeapon)
        {
            return false;
        }

        if(IsInSecondaryWeaponAttack())
        {
            return false;
        }

        return true;
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

    public void OnUseSecondaryWeapon()
    {
        if(!m_secondaryWeapon)
        {
            return;
        }

        if(m_currentMana - m_secondaryWeapon.m_manaCost < 0)
        {
            return;
        }

        AlterMana(-m_secondaryWeapon.m_manaCost);

        m_inuseSecondaryWeapon = m_secondaryWeapon;
        m_weaponSpawnCoroutine = StartCoroutine(UseSecondaryWeapon());
    }

    public void CarryOverAttack()
    {
        float timeInAnim = Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        Animator.Play("playerThrow", -1, timeInAnim);
    }

    void OnSpawnWeapon()
    {
        GameObject spawnedWeapon = Instantiate(m_inuseSecondaryWeapon.m_spawnedObject, transform.position, Quaternion.identity, transform.parent);

        MovementComponent movementComponent = GetComponent<MovementComponent>();

        spawnedWeapon.BroadcastMessage("OnSpawn", Mathf.Sign(transform.localScale.x));
    }

    void OnInterrupt()
    {
        if(m_weaponSpawnCoroutine != null)
        {
            StopCoroutine(m_weaponSpawnCoroutine);
            m_weaponSpawnCoroutine = null;
        }

        SecondaryWeaponUsageCleanup();
    }

    void SignalManaChanged()
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
        SignalManaChanged();
    }

    void RestoreMana()
    {
        m_currentMana = m_maxMana;
        SignalManaChanged();
    }

    void ResetMana()
    {
        m_currentMana = 0;
        SignalManaChanged();
    }

    public int GetMaxMana()
    {
        return m_maxMana;
    }

    IEnumerator UseSecondaryWeapon()
    {
        m_secondaryWeaponUsageSound.Play();

        Animator.SetTrigger(m_inuseSecondaryWeapon.m_animationTrigger);
        yield return new WaitForSeconds(m_inuseSecondaryWeapon.m_startUpDuration);

        // spawn item
        OnSpawnWeapon();
        yield return new WaitForSeconds(m_inuseSecondaryWeapon.m_timeInUsageDuration - m_inuseSecondaryWeapon.m_startUpDuration);

        SecondaryWeaponUsageCleanup();
    }

    void SecondaryWeaponUsageCleanup()
    {
        Animator.ResetTrigger(m_inuseSecondaryWeapon.m_animationTrigger);
        m_weaponSpawnCoroutine = null;
    }

    Animator Animator;
    SecondaryWeapon m_secondaryWeapon;

    // could have the case where item usage gets interrupted part way through coroutine, but the secondary weapon has changed
    // need to cache to make sure the proper weapon is cleaned up
    SecondaryWeapon m_inuseSecondaryWeapon;

    Coroutine m_weaponSpawnCoroutine;

    int m_currentMana;
}
