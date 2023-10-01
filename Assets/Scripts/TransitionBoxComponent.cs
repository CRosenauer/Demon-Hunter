using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TransitionBoxComponent : TransitionComponent
{
    new void Start()
    {
        base.Start();
        Collider2D collider = GetComponent<Collider2D>();
        Debug.Assert(collider.isTrigger);
    }

    void Update()
    {
        if (m_sceneLoader.isDone)
        {
            RebaseGameObjects();
            UnloadPreviousLevel();

            // todo: add animation on entering level
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = spawnPoint.transform.position;
            m_camera.transform.position = new(0f, 0f, -10f);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_camera = Camera.main;
        LoadLevel();
    }
}
