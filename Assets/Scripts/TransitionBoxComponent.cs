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
            FinishLoading(true);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_camera = Camera.main;
        LoadLevel();
    }
}
