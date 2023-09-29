using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BookComponent : SecondaryWeaponComponent
{
    public override void OnSpawn(float direction)
    {
        m_audioSource = GetComponent<AudioSource>();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        StartCoroutine(ScreenClear(enemies, m_audioSource, gameObject));
    }

    public static IEnumerator ScreenClear(GameObject[] enemies, AudioSource audioSource, GameObject objectToDestroy)
    {
        Time.timeScale = 0f;

        List<GameObject> onScreenEnemies = new();

        foreach (GameObject enemy in enemies)
        {
            if (MovementComponent.IsWithinCameraFrustum(enemy.transform))
            {
                onScreenEnemies.Add(enemy);
                enemy.BroadcastMessage("OnClear");
            }
        }

        audioSource.Play();

        yield return new WaitForSecondsRealtime(1f);

        foreach (GameObject enemy in onScreenEnemies)
        {
            Destroy(enemy);
        }

        if(objectToDestroy)
        {
            Destroy(objectToDestroy);
        }

        Time.timeScale = 1f;
    }

    AudioSource m_audioSource;
}
