using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraComponent : MonoBehaviour
{
    public enum TrackingAxis
    {
        X,
        Y
    }

    public enum CameraMovementMode
    {
        Snap,
        Linear,
    }

    [SerializeField] GameObject m_trackingObject;
    [SerializeField] TrackingAxis m_trackingAxis;
    [Space]

    [SerializeField] Vector3 m_cameraPositionOffset;

    [SerializeField] float m_minPosition;
    [SerializeField] float m_maxPosition;
    [SerializeField] bool m_boundLimitEnabled = true;
    [Space]

    [SerializeField] float m_offsetBuffer;

#if UNITY_EDITOR
    [Space]
    [SerializeField]
#endif
    CameraMovementMode m_movementMode;


    const float m_scrollSpeed = 5; // units / second

    void Start()
    {
        m_movementMode = CameraMovementMode.Snap;
        Debug.Assert(m_trackingObject);

        if(Camera.allCamerasCount > 1)
        {
            gameObject.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        CameraTrackUpdate();
    }

    void CameraTrackUpdate()
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
        switch (m_movementMode)
        {
            case CameraMovementMode.Snap:
                thisPosition = CalculateSnapTrackingCoordinate(thisPosition, trackedPosition);
                break;
            case CameraMovementMode.Linear:
                thisPosition = CalculateLinearTrackingCoordinate(thisPosition, trackedPosition);
                break;
        }

        if(m_boundLimitEnabled)
        {
            thisPosition = Mathf.Clamp(thisPosition, m_minPosition, m_maxPosition);
        }

        return thisPosition;
    }

    float CalculateSnapTrackingCoordinate(float thisPosition, float trackedPosition)
    {
        float trackedDistance = thisPosition - trackedPosition;
        
        if (trackedDistance > m_offsetBuffer)
        {
            return trackedPosition + m_offsetBuffer;
        }
        else if (trackedDistance < -m_offsetBuffer)
        {
            return trackedPosition - m_offsetBuffer;
        }

        return thisPosition;
    }

    float CalculateLinearTrackingCoordinate(float thisPosition, float trackedPosition)
    {
        trackedPosition = trackedPosition + m_offsetBuffer;
        float trackedDistance = trackedPosition - thisPosition;

        if(Mathf.Abs(trackedDistance) < m_scrollSpeed * Time.fixedDeltaTime)
        {
            return trackedPosition;
        }

        if (trackedDistance < 0)
        {
            return thisPosition - m_scrollSpeed * Time.fixedDeltaTime;
        }
        else if (trackedDistance > 0)
        {
            return thisPosition + m_scrollSpeed * Time.fixedDeltaTime;
        }

        return thisPosition;
    }

    void NewTrackingObject(GameObject obj)
    {
        m_trackingObject = obj;
    }

    void SetTrackingAxis(TrackingAxis trackingAxis)
    {
        m_trackingAxis = trackingAxis;
    }

    void SetOffsetPosition(Vector3 offset)
    {
        m_cameraPositionOffset = offset;
    }

    void SetPositionBounds(Vector2 bounds)
    {
        m_minPosition = bounds.x;
        m_maxPosition = bounds.y;
    }

    void EnableCameraBounds(bool enable)
    {
        m_boundLimitEnabled = enable;
    }

    void SetOffsetBuffer(float buffer)
    {
        m_offsetBuffer = buffer;
    }

    void ChangeCameraMovementMode(CameraMovementMode movementMode)
    {
        m_movementMode = movementMode;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        collision.SendMessage("OnEnterCamera");
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        collision.SendMessage("OnExitCamera");
    }
}
