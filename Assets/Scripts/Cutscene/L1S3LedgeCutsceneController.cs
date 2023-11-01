using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L1S3LedgeCutsceneController : MonoBehaviour
{
    [SerializeField] GameObject m_rumbleParticle;

    void Rumble()
    {
        StartCoroutine(RumbleCoroutine(0.5f));
    }

    void SpawnParticles()
    {
        float partcileLowRange = -2.25f;
        float partcileHighRange = 0.25f;

        Vector2 positon = transform.position;

        for(int i =0; i < 5; ++i)
        {
            float xPos = Random.Range(partcileLowRange, partcileHighRange);
            Vector2 pos = positon + Vector2.right * xPos;

            GameObject rubble = Instantiate(m_rumbleParticle, pos, Quaternion.identity);
            Destroy(rubble, 1f);
            Rigidbody2D rbody = rubble.GetComponent<Rigidbody2D>();
            rbody.angularVelocity = Random.Range(1, -1);
        }
    }

    void Fall()
    {
        Rigidbody2D rbody = GetComponent<Rigidbody2D>();
        rbody.bodyType = RigidbodyType2D.Dynamic;
    }

    IEnumerator RumbleCoroutine(float duration)
    {
        float stepTime = 0.05f;
        float stepPosition = 0.2f;

        Vector2 originalPosition = transform.position;

        Vector2 rumblePosition = transform.position;
        rumblePosition.x += stepPosition * 0.5f;

        float timeInRumble = 0f;

        while(timeInRumble < duration)
        {
            rumblePosition.x -= stepPosition;
            transform.position = rumblePosition;
            timeInRumble += stepTime;
            yield return new WaitForSeconds(stepTime);

            rumblePosition.x += stepPosition;
            transform.position = rumblePosition;
            timeInRumble += stepTime;
            yield return new WaitForSeconds(stepTime);
        }

        transform.position = originalPosition;
    }
}
