using System.Collections;
using System.Collections.Generic;
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

        m_player.SendMessage("SetCutscene", true);
        m_bossCultist.SendMessage("SetCutscene", true);
        m_player.SendMessage("Move", Vector2.zero);

        yield return new WaitForSeconds(1f);

        Animator cultistAnimator = m_bossCultist.GetComponent<Animator>();
        cultistAnimator.SetTrigger("Laugh");
        PlaySound(m_cultistLaugh);
        yield return new WaitForSeconds(2.67f);

        m_player.SendMessage("SetCutscene", false);
        m_bossCultist.SendMessage("SetCutscene", false);

        m_bossCultist.SendMessage("Activate");
    }

    GameObject m_player;
}
