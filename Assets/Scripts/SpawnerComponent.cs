using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerComponent : MonoBehaviour
{
    const int m_enemyLimit = 15;

    [SerializeField] GameObject m_spawnedObject;
    [Space]

    [SerializeField] List<SpawnPattern> m_spawnPatterns;

    [SerializeField] float m_minTime;
    [SerializeField] float m_maxTime;
    [Space]

    [SerializeField] float m_xMax;
    [SerializeField] float m_xMin;

    [SerializeField] float m_yPos;

    [SerializeField] float m_minDist;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(m_spawnedObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_player)
        {
            m_timer -= Time.deltaTime;

            if(m_timer <= 0f)
            {
                Vector3 position = new();

                position.x = Random.Range(m_xMin, m_xMax);

                if(Mathf.Abs(position.x) < m_minDist)
                {
                    m_timer = 0;
                    return;
                }

                position.x += m_player.transform.position.x;
                position.y = m_yPos;
                position.z = 0f;

                if (m_spawnPatterns.Count == 0)
                {
                    SpawnEnemy(m_spawnedObject, position);
                }
                else
                {
                    int spawnPatternIndex = Random.Range(0, m_spawnPatterns.Count);

                    SpawnPattern spawnPattern = m_spawnPatterns[spawnPatternIndex];

                    foreach(Vector3 offset in spawnPattern.m_positionOffsets)
                    {
                        SpawnEnemy(m_spawnedObject, position + offset);
                    }
                }

                for(int i = 0; i < m_spawnedEnemies.Count; ++i)
                {
                    if(!m_spawnedEnemies[i])
                    {
                        m_spawnedEnemies.RemoveAt(i);
                    }
                }

                while (m_spawnedEnemies.Count > m_enemyLimit)
                {
                    Destroy(m_spawnedEnemies[0]);
                    m_spawnedEnemies.RemoveAt(0);
                }

                m_timer = Random.Range(m_minTime, m_maxTime);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        LayerMask playerLayer = LayerMask.NameToLayer("Player");

        if(other.gameObject.layer == playerLayer)
        {
            m_player = other.gameObject;
            m_timer = Random.Range(m_minTime, m_maxTime);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        LayerMask playerLayer = LayerMask.NameToLayer("Player");

        if (other.gameObject.layer == playerLayer)
        {
            m_player = null;
        }
    }

    void SpawnEnemy(GameObject enemy, Vector3 position)
    {
        GameObject spawnedEnemy = Instantiate(enemy, position, Quaternion.identity);
        spawnedEnemy.AddComponent<DespawnComponent>();
        spawnedEnemy.SendMessage("SetDespawnTolerance", 200f);
        m_spawnedEnemies.Add(spawnedEnemy);
        spawnedEnemy.transform.SetParent(transform.parent);
    }

    List<GameObject> m_spawnedEnemies = new();
    GameObject m_player;

    float m_timer;
}
