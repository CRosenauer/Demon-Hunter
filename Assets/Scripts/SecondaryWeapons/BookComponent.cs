using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BookComponent : SecondaryWeaponComponent
{
    [SerializeField] int m_damage;

    public override void OnSpawn(float direction)
    {
        m_audioSource = GetComponent<AudioSource>();

        StartCoroutine(ScreenClear( m_audioSource, gameObject, m_damage));
    }

    public static IEnumerator ScreenClear(AudioSource audioSource, GameObject objectToDestroy, int damage)
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
            enemy.BroadcastMessage("OnClearEnd");
            enemy.SendMessage("OnHit", damage);
        }

        if(objectToDestroy)
        {;
            Destroy(objectToDestroy);
        }

        Time.timeScale = 1f;
    }

    AudioSource m_audioSource;
}
