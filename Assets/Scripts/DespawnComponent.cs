using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnComponent : MonoBehaviour
{
    void OnExitCamera()
    {
        Destroy(gameObject);
    }
}
