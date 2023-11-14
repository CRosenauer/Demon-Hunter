using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionComponent : MonoBehaviour
{
    [SerializeField] protected string m_scene;
    [SerializeField] protected GameObject m_level;
    protected void Start()
    {
        m_camera = Camera.main;

        Debug.Assert(m_scene != null);
        Debug.Assert(m_camera);
    }

    protected void LoadLevel()
    {
        // could accidentally double load areas
        if (m_triggered)
        {
            return;
        }

        // camera is not guarenteed to be cached by this point.
        // honestly a very hacky solution. FIX ME!

        m_camera = Camera.main;

        m_triggered = true;
        m_sceneLoader = SceneManager.LoadSceneAsync(m_scene, LoadSceneMode.Additive);
    }

    protected void RebaseGameObjects()
    {
        Scene loadedScene = SceneManager.GetSceneAt(1);

        SceneManager.MoveGameObjectToScene(m_player, loadedScene);
        SceneManager.MoveGameObjectToScene(m_camera.gameObject, loadedScene);

        GameObject obj = GameObject.Find("Core(Clone)");
        SceneManager.MoveGameObjectToScene(obj, loadedScene);
    }

    protected void UnloadPreviousLevel()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }

    protected void FinishLoading(bool moveToSpawnPoint)
    {
        RebaseGameObjects();
        CameraBootstrap.LoadCameraParams();
        UnloadPreviousLevel();

        if(moveToSpawnPoint)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

            GameObject spawnPoint = spawnPoints[0];

            int buildIndex = SceneManager.GetActiveScene().buildIndex;

            foreach (GameObject sp in spawnPoints)
            {
                if (sp.scene.buildIndex == buildIndex)
                {
                    spawnPoint = sp;
                    break;
                }
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = spawnPoint.transform.position;
        }
        
        m_camera.transform.position = new(0f, 0f, -10f);

        TransitionDoorComponent.SignalSceneLoadedActions();
    }

    protected Camera m_camera;
    protected GameObject m_player;
    protected AsyncOperation m_sceneLoader;
    protected bool m_triggered = false;
}
