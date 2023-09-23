using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] GameObject m_corePrefab;

    [Space]
    [SerializeField] GameObject m_playerPrefab;
    [SerializeField] GameObject m_cameraPrefab;

    // Start is called before the first frame update
    void Start()
    {
        CheckObjectExistance("MainCamera", m_cameraPrefab);
        GameObject player = CheckObjectExistance("Player", m_playerPrefab);

        if(player)
        {
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");

            if(spawnPoint)
            {
                player.transform.position = spawnPoint.transform.position;
            }
        }

        CheckCoreExistance("Core", m_corePrefab);
    }

    GameObject CheckObjectExistance(string tag, GameObject fallbackSpawnedObject)
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(tag);
        if (foundObjects.Length == 0)
        {
            return Instantiate(fallbackSpawnedObject, transform.parent);
        }

        return null;
    }

    void CheckCoreExistance(string name, GameObject fallbackPrespawnedObject)
    {
        GameObject core = GameObject.Find(name);
        if (!core)
        {
            Instantiate(fallbackPrespawnedObject, transform.parent);
        }
    }
}
