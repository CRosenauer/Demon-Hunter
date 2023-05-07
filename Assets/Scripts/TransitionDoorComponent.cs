using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionDoorComponent : MonoBehaviour
{
    const float c_cameraMoveSpeed = 3f;

    enum TransitionState
    {
        LoadScene,
        MoveCamera,
        DoorOpen,
        MovePlayer,
        DoorClose,
        FinishMoveCamera,
        ExitTransition
    }

    [SerializeField] string m_scene;

    [SerializeField] GameObject m_exitPoint;
    [SerializeField] GameObject m_level;

    [SerializeField] Camera m_camera;

    void Start()
    {
        m_animator = GetComponent<Animator>();

        Debug.Assert(m_scene != null);
        Debug.Assert(m_exitPoint);

        enabled = false;
    }

    void FixedUpdate()
    {
        // would prob be better to use eventss
        
        switch(m_state)
        {
            case TransitionState.LoadScene:
                LoadScene();
                break;
            case TransitionState.MoveCamera:
                MoveCamera();
                break;
            case TransitionState.DoorOpen:
                DoorOpen();
                break;
            case TransitionState.MovePlayer:
                MovePlayer();
                break;
            case TransitionState.DoorClose:
                DoorClose();
                break;
            case TransitionState.FinishMoveCamera:
                FinishMoveCamera();
                break;
            case TransitionState.ExitTransition:
                ExitTransition();
                break;
        }
    }

    void LoadScene()
    {
        if (m_sceneLoader.isDone)
        {
            m_state = TransitionState.MoveCamera;
        }
    }

    void MoveCamera()
    {
        Vector3 cameraPosition = m_camera.transform.position;
        Vector3 targetPosition = transform.position;

        float deltaToTargetPosition = targetPosition.x - cameraPosition.x;
        
        if(Mathf.Abs(deltaToTargetPosition) <= Time.fixedDeltaTime * c_cameraMoveSpeed)
        {
            cameraPosition.x = targetPosition.x;
            m_camera.transform.position = cameraPosition;

            m_state = TransitionState.MovePlayer;
            return;
        }

        float deltaX = targetPosition.x - cameraPosition.x;
        deltaX = Mathf.Sign(deltaX) * c_cameraMoveSpeed * Time.fixedDeltaTime;

        cameraPosition.x += deltaX;

        m_camera.transform.position = cameraPosition;
    }

    void DoorOpen()
    {
        m_animator.SetTrigger("OnOpen");

        AnimatorStateInfo animInfo = m_animator.GetCurrentAnimatorStateInfo(0);

        ;
    }

    void MovePlayer()
    {

    }

    void DoorClose()
    {
        m_animator.SetTrigger("OnClose");
    }

    void FinishMoveCamera()
    {
        // get level camera position
    }

    void ExitTransition()
    {
        // update camera serialized fields
        ReactivateGame();
        enabled = false;

        // signal clean up
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        m_sceneLoader = SceneManager.LoadSceneAsync(m_scene);

        m_state = TransitionState.LoadScene;

        m_player = other.gameObject;
        PlayerMovement playerMovementComponent = m_player.GetComponent<PlayerMovement>();
        playerMovementComponent.enabled = false;

        CameraComponent cameraComponent = m_camera.GetComponent<CameraComponent>();
        cameraComponent.enabled = false;

        m_level.SetActive(false);
        enabled = true;

        m_state = TransitionState.LoadScene;
    }

    void ReactivateGame()
    {
        PlayerMovement playerMovementComponent = m_player.GetComponent<PlayerMovement>();
        playerMovementComponent.enabled = true;

        CameraComponent cameraComponent = m_camera.GetComponent<CameraComponent>();
        cameraComponent.enabled = true;
    }

    GameObject m_player;
    AsyncOperation m_sceneLoader;

    Animator m_animator;

    TransitionState m_state;
}
