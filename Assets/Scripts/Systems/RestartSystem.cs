using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartSystem : MonoBehaviour
{
    void OnRestart()
    {
        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(currentScene);
    }
}
