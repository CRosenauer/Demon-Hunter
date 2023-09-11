using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeathBarComponent : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Assert(player);

        LifeComponent lifeComponent = player.GetComponent<LifeComponent>();
        Debug.Assert(lifeComponent);

        lifeComponent.OnHealthChanged += OnHealthChanged;

        m_healthBarTicks = new();

        foreach (Transform child in transform)
        {
            m_healthBarTicks.Add(child.gameObject);
        }

        Debug.Assert(m_healthBarTicks.Count == lifeComponent.GetMaxHealth());
    }

    void OnDestroy()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(!player)
        {
            return;
        }

        LifeComponent lifeComponent = player.GetComponent<LifeComponent>();
        if (!lifeComponent)
        {
            return;
        }

        lifeComponent.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(int health)
    {
        for(int i = 0; i < m_healthBarTicks.Count; ++i)
        {
            if (i < health)
            {
                m_healthBarTicks[i].active = true;
            }
            else
            {
                m_healthBarTicks[i].active = false;
            }
        }
    }

    List<GameObject> m_healthBarTicks;
}
