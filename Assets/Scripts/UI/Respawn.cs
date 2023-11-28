using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Respawn : MonoBehaviour
{
    [SerializeField] GameObject m_system;
    [SerializeField] AudioSource m_deathSound;

    [SerializeField] GameObject m_graphicsContainer;
    [SerializeField] GameObject m_respawnBackground;

    [SerializeField] LivesChangedEvent m_lives;

    void Start()
    {
        Debug.Assert(m_system);
        Debug.Assert(m_graphicsContainer);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement playerMovment = player.GetComponent<PlayerMovement>();
        playerMovment.OnPlayerDeath += OnActive;
        m_gameOver = false;
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
        m_lives.Raise();
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

        yield return Restart();
    }

    IEnumerator Restart()
    {
        yield return new WaitForSeconds(1f);

        if(!m_gameOver)
        {
            yield return new WaitForSeconds(1f);
            m_system.SendMessage("OnRestart");
        }
        else
        {
            Animator animator = m_respawnBackground.GetComponent<Animator>();
            animator.SetTrigger("gameover");
            yield return new WaitForSeconds(1.5f);
            SceneManager.LoadScene("Scenes/titleScreen", LoadSceneMode.Single);
        }
    }

    public void SignalLivesCount(int lives)
    {
        if(lives <= 0)
        {
            m_gameOver = true;
        }
    }

    bool m_gameOver;
}
