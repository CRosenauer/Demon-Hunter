using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBarComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Assert(player);

        SecondaryWeaponManagerComponent secondaryWeapon = player.GetComponent<SecondaryWeaponManagerComponent>();
        Debug.Assert(secondaryWeapon);

        secondaryWeapon.OnManaChanged += OnHManaChanged;

        m_manaBarTicks = new();

        // to do: make the entire UI element variable sized based on health count
        foreach (Transform child in transform)
        {
            m_manaBarTicks.Add(child.gameObject);
        }

        Debug.Assert(m_manaBarTicks.Count == secondaryWeapon.GetMaxMana());
    }

    void OnDestroy()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!player)
        {
            return;
        }

        SecondaryWeaponManagerComponent lifeComponent = player.GetComponent<SecondaryWeaponManagerComponent>();
        if (!lifeComponent)
        {
            return;
        }

        lifeComponent.OnManaChanged -= OnHManaChanged;
    }

    void OnHManaChanged(int health)
    {
        for (int i = 0; i < m_manaBarTicks.Count; ++i)
        {
            // likely a better way to do this. could revise in the future
            if (i < health)
            {
                m_manaBarTicks[i].SetActive(true);
            }
            else
            {
                m_manaBarTicks[i].SetActive(false);
            }
        }
    }

    List<GameObject> m_manaBarTicks;
}
