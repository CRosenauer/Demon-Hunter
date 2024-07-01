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
        GameObject HUD = GameObject.Find("HUD");
        HUD.BroadcastMessage("StopTimer");

        while (!m_player)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
            yield return new WaitForEndOfFrame();
        }

        m_playerMovementComponent = m_player.GetComponent<MovementComponent>();
        Debug.Assert(m_playerMovementComponent, "L1S4Exit.CutsceneCoroutine. MovementComponent doesn't exist on the player!");

        m_bossCultistMovementComponent = m_bossCultist.GetComponent<MovementComponent>();
        Debug.Assert(m_bossCultistMovementComponent, "L1S4Exit.CutsceneCoroutine. MovementComponent doesn't exist on the boss cultist!");

        List<GameObject> enemies = new(GameObject.FindGameObjectsWithTag("Enemy"));
        enemies.Remove(m_bossCultist);

        foreach(GameObject enemy in enemies)
        {
            enemy.BroadcastMessage("OnDeath");
        }

        m_playerMovementComponent.IsInCutscene = true;
        m_bossCultistMovementComponent.IsInCutscene = true;
        m_playerMovementComponent.Move(Vector2.zero);

        m_playerMovementComponent.Animator.SetFloat("Speed", 0);

        yield return new WaitForSeconds(1f);
        m_bossCultistMovementComponent.Animator.SetTrigger("GetUp");

        yield return new WaitForSeconds(1f);
        m_bossCultistMovementComponent.Animator.SetTrigger("Transform");

        yield return new WaitForSeconds(2f);

        Rigidbody2D cultistRBody = m_bossCultist.GetComponent<Rigidbody2D>();
        cultistRBody.simulated = false;

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
    MovementComponent m_playerMovementComponent;
    MovementComponent m_bossCultistMovementComponent;
}
