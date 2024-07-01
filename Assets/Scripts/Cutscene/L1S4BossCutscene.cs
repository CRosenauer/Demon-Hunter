using System.Collections;
using UnityEngine;

public class L1S4BossCutscene : Cutscene
{
    [SerializeField] GameObject m_bossCultist;
    [SerializeField] GameObject m_cultistLaugh;

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

        m_playerMovementComponent = m_player.GetComponent<MovementComponent>();
        Debug.Assert(m_playerMovementComponent, "L1S4BossCutscene.CutsceneCoroutine. MovementComponent doesn't exist on the player!");

        m_bossCultistMovementComponent = m_bossCultist.GetComponent<MovementComponent>();
        Debug.Assert(m_bossCultistMovementComponent, "L1S4BossCutscene.CutsceneCoroutine. MovementComponent doesn't exist on the boss cultist!");

        m_playerMovementComponent.IsInCutscene = true;
        m_bossCultistMovementComponent.IsInCutscene = true;
        m_playerMovementComponent.Move(Vector2.zero);

        m_playerMovementComponent.Animator.SetFloat("Speed", 0);

        yield return new WaitForSeconds(1f);

        m_bossCultistMovementComponent.Animator.SetTrigger("Laugh");
        PlaySound(m_cultistLaugh);
        yield return new WaitForSeconds(2.67f);

        m_playerMovementComponent.IsInCutscene = false;
        m_bossCultistMovementComponent.IsInCutscene = false;

        m_bossCultist.SendMessage("Activate");
    }

    GameObject m_player;
    MovementComponent m_playerMovementComponent;
    MovementComponent m_bossCultistMovementComponent;
}
