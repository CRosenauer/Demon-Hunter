using UnityEngine;

public class DespawnComponent : MonoBehaviour
{
    void OnExitCamera()
    {
        Destroy(gameObject);
    }
}
