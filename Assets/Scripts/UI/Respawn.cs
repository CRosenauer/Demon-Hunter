using System.Collections;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] GameObject m_system;
    [SerializeField] AudioSource m_deathSound;

    [SerializeField] GameObject m_graphicsContainer;

    void Start()
    {
        Debug.Assert(m_system);
        Debug.Assert(m_graphicsContainer);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement playerMovment = player.GetComponent<PlayerMovement>();
        playerMovment.OnPlayerDeath += OnActive;
    }

    void OnActive()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement playerMovment = player.GetComponent<PlayerMovement>();

        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        m_system.SendMessage("ToggleMusic", false);
        if (m_deathSound)
        {
            m_deathSound.Play();
        }

        yield return new WaitForSeconds(4f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Destroy(player);
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        m_graphicsContainer.SetActive(true);

        DecrementLives();

        if(IsOutOfLives())
        {
            GameOver();
        }
        else
        {
            yield return Restart();
        }
    }

    void DecrementLives()
    {

    }

    bool IsOutOfLives()
    {
        return false;
    }

    void GameOver()
    {
        
    }

    IEnumerator Restart()
    {
        yield return new WaitForSeconds(1f);

        m_system.SendMessage("OnRestart");
    }
}
