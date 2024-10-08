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
        GameObject player = CheckObjectExistance("Player", m_playerPrefab);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach(GameObject enemy in enemies)
        {
            enemy.SendMessage("SetPlayer", player);
        }

        CheckObjectExistance("MainCamera", m_cameraPrefab);
        CameraBootstrap.LoadCameraParams();

        if(player)
        {
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");

            if(spawnPoint)
            {
                player.transform.position = spawnPoint.transform.position;
            }
        }

        // a bit hacky.
        // if we need to create the core components it means we're hard loading into this scene
        // no transition door will activate scene loaded actions! so we load them here
        if(CheckCoreExistance("Core(Clone)", m_corePrefab))
        {
            TransitionDoorComponent.SignalSceneLoadedActions();
        }
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

    // returns if an object was created
    bool CheckCoreExistance(string name, GameObject fallbackPrespawnedObject)
    {
        GameObject core = GameObject.Find(name);
        if (!core)
        {
            Instantiate(fallbackPrespawnedObject, transform.parent);
            return true;
        }

        return false;
    }
}
