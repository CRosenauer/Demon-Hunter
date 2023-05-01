using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraComponent : MonoBehaviour
{
    enum TrackingAxis
    {
        X,
        Y
    }

    [SerializeField] GameObject m_trackingObject;
    [SerializeField] TrackingAxis m_trackingAxis;
    [Space]

    [SerializeField] Vector3 m_cameraPositionOffset;

    [SerializeField] float m_minPosition;
    [SerializeField] float m_maxPosition;
    [Space]

    [SerializeField] float m_offsetBuffer;


    void Start()
    {
        Debug.Assert(m_trackingObject);
    }

    void FixedUpdate()
    {
        Vector3 trackedObjectPos = m_trackingObject.transform.position + m_cameraPositionOffset;

        Vector3 newCameraPos = transform.position;

        switch (m_trackingAxis)
        {
            case TrackingAxis.X:
                newCameraPos.x = CalculateNewTrackingCoordinate(newCameraPos.x, trackedObjectPos.x);
                break;
            case TrackingAxis.Y:
                newCameraPos.y = CalculateNewTrackingCoordinate(newCameraPos.y, trackedObjectPos.y);
                break;
        }

        transform.position = newCameraPos;
    }

    float CalculateNewTrackingCoordinate(float thisPosition, float trackedPosition)
    {
        float trackedDistance = thisPosition - trackedPosition;

        if (trackedDistance > m_offsetBuffer)
        {
            thisPosition = trackedPosition + m_offsetBuffer;
        }
        else if (trackedDistance < -m_offsetBuffer)
        {
            thisPosition = trackedPosition - m_offsetBuffer;
        }

        thisPosition = Mathf.Clamp(thisPosition, m_minPosition, m_maxPosition);

        return thisPosition;
    }
}
