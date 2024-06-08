using System.Collections;
using UnityEngine;

public class L1S4FallingIntro : LevelLoadCutscene
{
    [SerializeField] GameObject m_startPoint;
    [SerializeField] GameObject m_endPoint;
    [SerializeField] GameObject m_bushObject;
    [SerializeField] GameObject m_bushAudio;
    [SerializeField] float m_speed;
    [SerializeField] float m_jumpSpeed;

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
        Debug.Assert(m_playerMovementComponent, "L1S4FallingIntro.CutsceneCoroutine. MovementComponent doesn't exist on the player!");

        m_playerMovementComponent.SetCutscene(true);
        m_playerMovementComponent.Move(Vector2.zero);
        Rigidbody2D rbody = m_player.GetComponent<Rigidbody2D>();
        rbody.simulated = false;

        Animator playerAnimator = m_player.GetComponent<Animator>();
        playerAnimator.SetTrigger("OnJump");

        Renderer bushRenderer = m_bushObject.GetComponent<Renderer>();
        bushRenderer.sortingOrder = 8;

        m_player.transform.position = m_startPoint.transform.position;

        while (!MoveTick(m_player, m_endPoint, m_speed, Time.fixedDeltaTime))
        {
            yield return new WaitForFixedUpdate();
        }

        PlaySound(m_bushAudio);
        playerAnimator.ResetTrigger("OnJump");

        yield return new WaitForSeconds(0.5f);

        rbody.simulated = true;
        Vector2 velocity = rbody.velocity;
        velocity.y = m_jumpSpeed;
        rbody.velocity = velocity;

        yield return new WaitForSeconds(0.2f);
        bushRenderer.sortingOrder = 5;

        m_playerMovementComponent.SetCutscene(false);
    }

    GameObject m_player;
    MovementComponent m_playerMovementComponent;
}
