using UnityEngine;

public class SceneLoadAction : MonoBehaviour
{
    [SerializeField] BoxCollider2D m_collider;

    void OnSceneLoad()
    {
        m_collider.enabled = true;
    }
}
