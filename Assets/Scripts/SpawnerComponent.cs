using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerComponent : MonoBehaviour
{
    const int m_enemyLimit = 10;

    [SerializeField] GameObject m_spawnedObject;
    [Space]

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

                GameObject spawnedEnemy = Instantiate(m_spawnedObject, position, Quaternion.identity);
                m_spawnedEnemies.Add(spawnedEnemy);
                spawnedEnemy.transform.SetParent(transform.parent);


                if (m_spawnedEnemies.Count > m_enemyLimit)
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
        m_player = other.gameObject;
        m_timer = Random.Range(m_minTime, m_maxTime);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        m_player = null;
    }

    List<GameObject> m_spawnedEnemies = new();
    GameObject m_player;

    float m_timer;
}
