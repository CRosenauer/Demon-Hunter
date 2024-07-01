using System.Collections;
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

        MovementComponent playerMovementComponent = m_player.GetComponent<MovementComponent>();
        playerMovementComponent.IsInCutscene = true;
        playerMovementComponent.Move(Vector2.zero);

        yield return new WaitForEndOfFrame();

        Animator animator = m_player.GetComponent<Animator>();
        animator.SetTrigger("OnJumpEnd");
        animator.SetBool("CutsceneForceNonJump", true);
        animator.ResetTrigger("OnJump");
        animator.SetFloat("Speed", 3);

        Rigidbody2D rbody = m_player.GetComponent<Rigidbody2D>();
        rbody.simulated = false;

        m_player.transform.position = m_startPoint.transform.position;

        while(!MoveTick(m_player, m_endPoint, m_speed, Time.fixedDeltaTime))
        {
            yield return new WaitForFixedUpdate();
        }

        playerMovementComponent.IsInCutscene = false;
        rbody.simulated = true;
        animator.ResetTrigger("OnJumpEnd");
        animator.SetBool("CutsceneForceNonJump", false);
    }

    GameObject m_player;
}
