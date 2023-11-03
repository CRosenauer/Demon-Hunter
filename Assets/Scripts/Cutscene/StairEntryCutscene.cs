using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairEntryCutscene : LevelLoadCutscene
{
    [SerializeField] GameObject m_startPoint;
    [SerializeField] GameObject m_endPoint;
    [SerializeField] float m_speed;

    protected override void CutsceneFn()
    {
        StartCoroutine(CutsceneCoroutine());
    }

    IEnumerator CutsceneCoroutine()
    {
        while(!m_player)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
            yield return new WaitForEndOfFrame();
        }

        m_player.SendMessage("SetCutscene", true);
        m_player.SendMessage("Move", Vector2.zero);
        Animator animator = m_player.GetComponent<Animator>();
        animator.SetFloat("Speed", 3);

        Rigidbody2D rbody = m_player.GetComponent<Rigidbody2D>();
        rbody.simulated = false;

        m_player.transform.position = m_startPoint.transform.position;

        while(!MoveTick(m_player, m_endPoint, m_speed, Time.fixedDeltaTime))
        {
            yield return new WaitForFixedUpdate();
        }

        m_player.SendMessage("SetCutscene", false);
        rbody.simulated = true;
    }

    GameObject m_player;
}
