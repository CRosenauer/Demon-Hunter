using UnityEngine;

public class TriggerCameraParams : TriggerCallback
{
    // todo: integrate this with camera bootstrap
    [SerializeField] GameObject m_trackedObject;
    [SerializeField] CameraComponent.TrackingAxis m_trackingAxis;
    [SerializeField] Vector3 m_cameraPositionOffset;
    [SerializeField] Vector2 m_positionBounds;
    [SerializeField] bool m_boundLimitEnabled;
    [SerializeField] float m_offsetBuffer;
    [SerializeField] CameraComponent.CameraMovementMode m_movementMode;

    protected override void Callback()
    {
        Camera camera = Camera.main;

        if(m_trackedObject)
        {
            camera.SendMessage("NewTrackingObject", m_trackedObject);
        }
        camera.SendMessage("SetTrackingAxis", m_trackingAxis);
        camera.SendMessage("SetOffsetPosition", m_cameraPositionOffset);
        camera.SendMessage("SetPositionBounds", m_positionBounds);
        camera.SendMessage("EnableCameraBounds", m_boundLimitEnabled);
        camera.SendMessage("SetOffsetBuffer", m_offsetBuffer);
        camera.SendMessage("ChangeCameraMovementMode", m_movementMode);
    }
}
