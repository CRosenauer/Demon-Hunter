using System.Collections.Generic;
using UnityEngine;

public class SpawnerComponent : MonoBehaviour
{
    [SerializeField] int m_enemyLimit;
    [SerializeField] bool m_spawnIfWillExceedLimit;
    [SerializeField] bool m_absoluteWorldPosition;

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
                for (int i = 0; i < m_spawnedEnemies.Count; ++i)
                {
                    if (!m_spawnedEnemies[i])
                    {
                        m_spawnedEnemies.RemoveAt(i);
                    }
                }

                Vector3 position = new();

                position.x = Random.Range(m_xMin, m_xMax);

                if(m_absoluteWorldPosition)
                {
                    if (Mathf.Abs(position.x - m_player.transform.position.x) < m_minDist)
                    {
                        m_timer = 0;
                        return;
                    }
                }
                else
                {
                    if(Mathf.Abs(position.x) < m_minDist)
                    {
                        m_timer = 0;
                        return;
                    }
                }

                position.x = m_absoluteWorldPosition? position.x :  position.x += m_player.transform.position.x;
                position.y = m_yPos;
                position.z = 0f;

                if (m_spawnPatterns.Count == 0)
                {
                    if(!m_spawnIfWillExceedLimit)
                    {
                        if(m_spawnedEnemies.Count + 1 > m_enemyLimit)
                        {
                            SetTimer();
                            return;
                        }
                    }

                    SpawnEnemy(m_spawnedObject, position);
                }
                else
                {
                    int spawnPatternIndex = Random.Range(0, m_spawnPatterns.Count);

                    SpawnPattern spawnPattern = m_spawnPatterns[spawnPatternIndex];

                    if (!m_spawnIfWillExceedLimit)
                    {
                        if (m_spawnedEnemies.Count + spawnPattern.m_positionOffsets.Count > m_enemyLimit)
                        {
                            SetTimer();
                            return;
                        }
                    }

                    foreach (Vector3 offset in spawnPattern.m_positionOffsets)
                    {
                        SpawnEnemy(m_spawnedObject, position + offset);
                    }
                }

                while (m_spawnedEnemies.Count > m_enemyLimit)
                {
                    Destroy(m_spawnedEnemies[0]);
                    m_spawnedEnemies.RemoveAt(0);
                }

                SetTimer();
            }
        }
    }

    void SetTimer()
    {
        m_timer = Random.Range(m_minTime, m_maxTime);
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
        m_spawnedEnemies.Add(spawnedEnemy);
        spawnedEnemy.transform.SetParent(transform.parent);
    }

    List<GameObject> m_spawnedEnemies = new();
    GameObject m_player;

    float m_timer;
}
