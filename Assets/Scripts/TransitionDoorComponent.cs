using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionDoorComponent : TransitionComponent
{
    const float c_cameraMoveSpeed = 3f;

    enum TransitionState
    {
        Idle,
        LoadScene,
        MoveCamera,
        DoorOpen,
        DoorOpenEnd,
        MovePlayer,
        DoorClose,
        DoorCloseEnd,
        FinishMoveCamera,
        ExitTransition
    }

    [SerializeField] GameObject m_exitPoint;

    [SerializeField] Vector2 m_exitDoorPosition;

    public static void SignalSceneLoadedActions()
    {
        GameObject[] sceneLoadedActionObjects = GameObject.FindGameObjectsWithTag("SceneLoadAction");

        foreach (GameObject go in sceneLoadedActionObjects)
        {
            go.SendMessage("OnSceneLoad");
        }
    }

    new void Start()
    {
        base.Start();

        m_currentObjects = new();
        m_animator = GetComponent<Animator>();

        Debug.Assert(m_exitPoint);
    }

    void FixedUpdate()
    {
        // would prob be better to use eventss
        
        switch(m_state)
        {
            case TransitionState.Idle:
                break;
            case TransitionState.LoadScene:
                LoadScene();
                break;
            case TransitionState.MoveCamera:
                MoveCamera();
                break;
            case TransitionState.DoorOpen:
                DoorOpen();
                break;
            case TransitionState.DoorOpenEnd:
                DoorOpenEnd();
                break;
            case TransitionState.MovePlayer:
                MovePlayer();
                break;
            case TransitionState.DoorClose:
                DoorClose();
                break;
            case TransitionState.DoorCloseEnd:
                DoorCloseEnd();
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
            m_camera.BroadcastMessage("EnableCameraBounds", false);

            Vector3 cameraPosition = m_camera.transform.position;
            cameraPosition.y = m_cameraYPos;
            m_camera.transform.position = cameraPosition;

            // move all entities to the proper position
            Vector3 entranceDoorPosition = transform.position;
            Vector3 exitDoorPosition3D = new(m_exitDoorPosition.x, m_exitDoorPosition.y, 0f);

            Vector3 deltaPosition = exitDoorPosition3D - entranceDoorPosition;

            GameObject[] backgroundObjects = GameObject.FindGameObjectsWithTag("Background");

            foreach(GameObject go in backgroundObjects)
            {
                go.SendMessage("UpdateCameraStartPosition", deltaPosition);
            }

            foreach (GameObject obj in m_currentObjects)
            {
                obj.transform.position = obj.transform.position + deltaPosition;
            }

            // ensure proper camera is enabled
            foreach(Camera camera in Camera.allCameras)
            {
                camera.gameObject.SetActive(false);
            }
            m_camera.gameObject.SetActive(true);

            m_state = TransitionState.MoveCamera;

            m_nextSceneEnemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach(GameObject enemy in m_nextSceneEnemies)
            {
                enemy.SetActive(false);
            }
        }
    }

    void MoveCamera()
    {
        MovementComponent.MovementState movementState = m_player.GetComponent<PlayerMovement>().GetMovmentState();

        if(movementState != MovementComponent.MovementState.idle)
        {
            return;
        }

        Vector3 cameraPosition = m_camera.transform.position;
        Vector3 targetPosition = transform.position;

        float deltaToTargetPosition = targetPosition.x - cameraPosition.x;
        
        if(Mathf.Abs(deltaToTargetPosition) <= Time.fixedDeltaTime * c_cameraMoveSpeed)
        {
            cameraPosition.x = targetPosition.x;
            m_camera.transform.position = cameraPosition;

            m_state = TransitionState.DoorOpen;
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
        m_state = TransitionState.DoorOpenEnd;
    }

    void DoorOpenEnd()
    {
        AnimatorStateInfo animInfo = m_animator.GetCurrentAnimatorStateInfo(0);

        if(animInfo.IsName("doorStayOpen"))
        {
            // m_player.GetComponent<BoxCollider2D>().enabled = false;

            float distanceToExitPoint = m_exitPoint.transform.position.x - m_player.transform.position.x;
            m_player.BroadcastMessage("ForceEmulatedUserInput", new Vector2(Mathf.Sign(distanceToExitPoint), 0f));

            m_state = TransitionState.MovePlayer;
        }
    }

    void MovePlayer()
    {
        Vector3 playerPosition = m_player.transform.position;
        Vector3 exitPosition = m_exitPoint.transform.position;

        float distanceToExit = exitPosition.x - playerPosition.x;

        Vector2 playerVelocity = m_player.GetComponent<Rigidbody2D>().velocity;

        // is player is moving away from the exit point
        if(distanceToExit * playerVelocity.x < 0f)
        {
            m_player.BroadcastMessage("ForceEmulatedUserInput", new Vector2(0f, 0f));
            m_state = TransitionState.DoorClose;
        }
    }

    void DoorClose()
    {
        m_animator.SetTrigger("OnClose");
        m_state = TransitionState.DoorCloseEnd;
    }

    void DoorCloseEnd()
    {
        AnimatorStateInfo animInfo = m_animator.GetCurrentAnimatorStateInfo(0);

        if (animInfo.IsName("doorStayClosed"))
        {
            m_state = TransitionState.FinishMoveCamera;
        }
    }

    void FinishMoveCamera()
    {
        Vector3 cameraPosition = m_camera.transform.position;

        float deltaToTargetPosition = 0f - cameraPosition.x;

        if (Mathf.Abs(deltaToTargetPosition) <= Time.fixedDeltaTime * c_cameraMoveSpeed)
        {
            cameraPosition.x = 0f;
            m_camera.transform.position = cameraPosition;

            m_state = TransitionState.ExitTransition;
            return;
        }

        float deltaX = 0f - cameraPosition.x;
        deltaX = Mathf.Sign(deltaX) * c_cameraMoveSpeed * Time.fixedDeltaTime;

        cameraPosition.x += deltaX;

        m_camera.transform.position = cameraPosition;
    }

    void ExitTransition()
    {
        // update camera serialized fields
        ReactivateGame();
        m_state = TransitionState.Idle;
        enabled = false;

        TransitionScene();

        SignalSceneLoadedActions();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        LoadLevel();

        m_state = TransitionState.LoadScene;

        m_player = other.gameObject;
        m_player.BroadcastMessage("SetControl", false);

        CameraComponent cameraComponent = m_camera.GetComponent<CameraComponent>();
        cameraComponent.enabled = false;
        m_cameraYPos = m_camera.transform.position.y;

        m_level.SetActive(false);

        SceneManager.GetActiveScene().GetRootGameObjects(m_currentObjects);

        m_state = TransitionState.LoadScene;
    }

    void ReactivateGame()
    {
        foreach (GameObject enemy in m_nextSceneEnemies)
        {
            enemy.SetActive(true);
        }

        m_player.BroadcastMessage("SetControl", true);

        CameraComponent cameraComponent = m_camera.GetComponent<CameraComponent>();
        cameraComponent.enabled = true;

        m_camera.BroadcastMessage("EnableCameraBounds", true);
    }

    List<GameObject> m_currentObjects;

    GameObject[] m_nextSceneEnemies;

    Animator m_animator;

#if UNITY_EDITOR
    [SerializeField]
# endif
    TransitionState m_state = TransitionState.Idle;

    float m_cameraYPos;
}
