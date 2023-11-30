using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBootstrap : MonoBehaviour
{
    [SerializeField] CameraComponent.TrackingAxis m_trackingAxis;
    [SerializeField] Vector3 m_cameraPositionOffset;
    [SerializeField] Vector2 m_positionBounds;
    [SerializeField] bool m_boundLimitEnabled;
    [SerializeField] float m_offsetBuffer;
    [SerializeField] CameraComponent.CameraMovementMode m_movementMode;

    void OnLoad()
    {
        Camera camera = Camera.main;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        camera.SendMessage("NewTrackingObject", player);
        camera.SendMessage("SetTrackingAxis", m_trackingAxis);
        camera.SendMessage("SetOffsetPosition", m_cameraPositionOffset);
        camera.SendMessage("SetPositionBounds", m_positionBounds);
        camera.SendMessage("EnableCameraBounds", m_boundLimitEnabled);
        camera.SendMessage("SetOffsetBuffer", m_offsetBuffer);
        camera.SendMessage("ChangeCameraMovementMode", m_movementMode);

        camera.SendMessage("FixedUpdate");
    }

    public static void LoadCameraParams()
    {
        GameObject cameraBootstrap = GameObject.FindGameObjectWithTag("CameraBootstrap");

        if(cameraBootstrap)
        {
            cameraBootstrap.SendMessage("OnLoad");
            Destroy(cameraBootstrap);
        }
    }
}
