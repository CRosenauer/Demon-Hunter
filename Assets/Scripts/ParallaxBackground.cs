using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] float m_parallaxScale;
    [SerializeField] bool m_forceSetCameraStartPosition;
    [SerializeField] Vector2 m_forcedStartPosition;

    // Start is called before the first frame update
    void Start()
    {
        SetActiveCamera();
    }

    // Update is called once per frame
    void Update()
    {
        // should really be event-based
        // if project continues have camera object updated performed through scriptable objects.
        if(!m_camera)
        {
            SetActiveCamera();
            return;
        }

        UpdateThisPosition();
    }

    void SetActiveCamera()
    {
        m_camera = Camera.main;
        if(m_camera)
        {
            m_cameraStartPosition = m_forceSetCameraStartPosition ? m_forcedStartPosition : m_camera.transform.position;
        }
    }

    void UpdateThisPosition()
    {
        Vector2 cameraDeltaPosition = (Vector2) m_camera.transform.position - m_cameraStartPosition;

        float deltaX = cameraDeltaPosition.x;
        deltaX *= m_parallaxScale;

        transform.position = (Vector2) transform.parent.position + Vector2.right * deltaX;
    }

    void UpdateCameraStartPosition(Vector3 delta)
    {
        m_cameraStartPosition = m_cameraStartPosition + (Vector2) delta;
    }

    Camera m_camera;
    Vector2 m_cameraStartPosition;
}
