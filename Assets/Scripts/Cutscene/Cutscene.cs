using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Cutscene : MonoBehaviour
{
    void PlayCutscene()
    {
        if(m_running)
        {
            return;
        }

        m_running = true;
        CutsceneFn();
    }

    // returns if target has reached the location
    protected bool MoveTick(GameObject go, GameObject target, float speed, float delta)
    {
        Vector2 direction = target.transform.position - go.transform.position;
        Vector2 velocity = speed * direction.normalized;
        Vector2 displacement = velocity * delta;

        Vector2 goPositionAfterDisplacement = (Vector2) go.transform.position + displacement;
        Vector2 directionAfterDisplacement = (Vector2) target.transform.position - goPositionAfterDisplacement;

        bool passedTarget = Vector2.Dot(directionAfterDisplacement, direction) < 0f;

        if(passedTarget)
        {
            go.transform.position = target.transform.position;
            return true;
        }

        go.transform.position = goPositionAfterDisplacement;
        return false;
    }

    protected void PlaySound(GameObject soundObject)
    {
        AudioSource audioSource = soundObject.GetComponent<AudioSource>();
        audioSource.Play();
    }

    protected abstract void CutsceneFn();

    protected bool m_running;
}
