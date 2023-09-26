using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnComponent : MonoBehaviour
{
    void Update()
    {
        if(!MovementComponent.IsWithinCameraFrustum(gameObject.transform, m_tolerance))
        {
            Destroy(gameObject);
        }
    }

    void SetDespawnTolerance(float f)
    {
        m_tolerance = f;
    }

    float m_tolerance = 0f;
}
