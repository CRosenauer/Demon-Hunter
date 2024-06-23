using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : Button
{
    [SerializeField] string m_sceneToLoad;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(m_sceneToLoad.Length != 0);
    }

    public override void OnSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(m_sceneToLoad, LoadSceneMode.Single);
    }
}
