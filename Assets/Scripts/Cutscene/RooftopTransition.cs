using System.Collections;
using UnityEngine;

public class RooftopTransition : Cutscene
{
    [SerializeField] GameObject m_bossCultist;
    [Space]

    [SerializeField] GameObject m_cameraTrackingTarget;
    [Space]

    [SerializeField] GameObject m_cultistStartPoint;
    [SerializeField] GameObject m_hoverEndPoint;
    [SerializeField] GameObject m_attackTarget;
    [Space]

    [SerializeField] GameObject m_cultistLaugh;
    [Space]

    [SerializeField] GameObject m_energyBall;
    [SerializeField] GameObject m_energyBallHitSfx;
    [Space]

    [SerializeField] GameObject m_ledgeObject;

    protected override void CutsceneFn()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        m_playerMovementComponent = player.GetComponent<MovementComponent>();
        Debug.Assert(m_playerMovementComponent, "RooftopTransition.CutsceneFn. MovementComponent doesn't exist on the player!");

        m_playerAnimator = player.GetComponent<Animator>();

        m_bossCultistMovementComponent = m_bossCultist.GetComponent<MovementComponent>();
        Debug.Assert(m_bossCultistMovementComponent, "RooftopTransition.CutsceneFn. MovementComponent doesn't exist on the boss cultist!");

        m_camera = Camera.main;

        StartCoroutine(CutsceneCoroutine());
    }

    IEnumerator CutsceneCoroutine()
    {
        m_playerMovementComponent.SetCutscene(true);
        m_playerMovementComponent.Move(Vector2.zero);

        m_playerAnimator.SetFloat("Speed", 0);

        m_bossCultistMovementComponent.SetCutscene(true);

        m_camera.SendMessage("ChangeCameraMovementMode", CameraComponent.CameraMovementMode.Linear);
        m_camera.SendMessage("NewTrackingObject", m_cameraTrackingTarget);

        yield return new WaitForSeconds(0.65f);

        m_bossCultist.transform.position = m_cultistStartPoint.transform.position;

        while (!MoveTick(m_bossCultist, m_hoverEndPoint, 2, Time.fixedDeltaTime))
        {
            yield return new WaitForFixedUpdate();
        }

        // cultist laugh
        Animator cultistAnimator = m_bossCultist.GetComponent<Animator>();
        cultistAnimator.SetTrigger("Laugh");
        PlaySound(m_cultistLaugh);

        yield return new WaitForSeconds(2f);
        cultistAnimator.ResetTrigger("Laugh");

        // cultist attack 1
        cultistAnimator.SetTrigger("Summon");
        StartCoroutine(CultistAttackCoroutine(0.33f, false, true));
        StartCoroutine(CultistAttackCoroutine(0.66f));
        StartCoroutine(CultistAttackCoroutine(1f, true));

        while(!m_attacksDone)
        {
            yield return new WaitForFixedUpdate();
        }

        // terrain break and player falls
        m_ledgeObject.SendMessage("Fall");
        m_playerMovementComponent.SetCutscene(false);
    }

    IEnumerator CultistAttackCoroutine(float delay, bool trackRunning = false, bool startRumble = false)
    {
        yield return new WaitForSeconds(delay);
        GameObject energyBall = Instantiate(m_energyBall, m_bossCultist.transform.position, Quaternion.identity);

        while (!MoveTick(energyBall, m_attackTarget, 4, Time.fixedDeltaTime))
        {
            yield return new WaitForFixedUpdate();
        }

        PlaySound(m_energyBallHitSfx);
        Destroy(energyBall);

        m_ledgeObject.SendMessage("SpawnParticles");

        if(startRumble)
        {
            m_ledgeObject.SendMessage("Rumble");
        }

        if (trackRunning)
        {
            m_attacksDone = true;
        }
    }

    bool m_attacksDone = false;

    Camera m_camera;
    Animator m_playerAnimator;
    MovementComponent m_playerMovementComponent;
    MovementComponent m_bossCultistMovementComponent;
}
