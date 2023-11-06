using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class L1S4Exit : Cutscene
{
    [SerializeField] GameObject m_bossCultist;
    [SerializeField] GameObject m_bossExitPoint;
    [SerializeField] GameObject m_exitSFX;
    [SerializeField] GameObject m_fadeOut;

    protected override void CutsceneFn()
    {
        StartCoroutine(CutsceneCoroutine());
    }

    IEnumerator CutsceneCoroutine()
    {
        while (!m_player)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
            yield return new WaitForEndOfFrame();
        }

        List<GameObject> enemies = new(GameObject.FindGameObjectsWithTag("Enemy"));
        enemies.Remove(m_bossCultist);

        foreach(GameObject enemy in enemies)
        {
            enemy.BroadcastMessage("OnDeath");
        }

        m_player.SendMessage("SetCutscene", true);
        m_bossCultist.SendMessage("SetCutscene", true);
        m_player.SendMessage("Move", Vector2.zero);

        Animator playerAnimator = m_player.GetComponent<Animator>();
        playerAnimator.SetFloat("Speed", 0);

        Rigidbody2D cultistRBody = m_bossCultist.GetComponent<Rigidbody2D>();
        cultistRBody.simulated = false;

        Animator cultistAnimator = m_bossCultist.GetComponent<Animator>();

        yield return new WaitForSeconds(1f);
        cultistAnimator.SetTrigger("GetUp");

        yield return new WaitForSeconds(1f);
        cultistAnimator.SetTrigger("Transform");

        yield return new WaitForSeconds(2f);

        PlaySound(m_exitSFX);
        while (!MoveTick(m_bossCultist, m_bossExitPoint, 10, Time.fixedDeltaTime))
        {
            yield return new WaitForFixedUpdate();
        }

        // fade out
        m_fadeOut.SendMessage("FadeOut");
        yield return new WaitForSeconds(3f);

        // return to title
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scenes/titleScreen", LoadSceneMode.Single);
    }

    GameObject m_player;
}
