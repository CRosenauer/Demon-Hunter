using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCultistZombieSpawner : MonoBehaviour
{
    [SerializeField] GameObject m_enemy;
    [SerializeField] GameObject m_spawnPoint;

    void Start()
    {
        Debug.Assert(m_enemy);
        Debug.Assert(m_spawnPoint);
    }

    void SpawnObject()
    {
        Instantiate(m_enemy, m_spawnPoint.transform.position, Quaternion.identity, transform.parent);
        Destroy(gameObject);
    }
}
