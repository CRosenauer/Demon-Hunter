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

        Camera camera = Camera.main;
        Collider2D onCameraCollider = camera.GetComponent<Collider2D>();
        List<Collider2D> onCameraObjects = new();
        onCameraCollider.GetContacts(onCameraObjects);

        foreach(Collider2D collider in onCameraObjects)
        {
            GameObject go = collider.gameObject;

            if(go.tag == "Enemy" || go.tag == "EnemyProjectile")
            {
                onScreenEnemies.Add(go);
                go.BroadcastMessage("OnClear");
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
