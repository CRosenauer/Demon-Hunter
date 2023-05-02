using UnityEngine;

public class StairComponent : MonoBehaviour
{
    [SerializeField] GameObject m_destinationStair;
    [SerializeField] GameObject m_upperCollider;
    [Space]

    [SerializeField] float m_stairMovementThreshold;

    void Start()
    {
        m_dirToOtherStair = m_destinationStair.transform.position - transform.position;
        m_dirToOtherStair.Normalize();
    }

    public Vector2 CalculateToStairMovement(Vector3 playerPosition, Vector2 userInput, float speed)
    {
        userInput.x = 0f;

        float dot = Vector2.Dot(userInput, m_dirToOtherStair);

        if(dot < 0.01f)
        {
            return Vector2.zero;
        }

        Vector3 playerToStairEnterPos = transform.position - playerPosition;

        if (playerToStairEnterPos.sqrMagnitude <= m_stairMovementThreshold * m_stairMovementThreshold)
        {
            return Vector2.zero;
        }

        Vector3 playerToStairR3 = transform.position - playerPosition;
        Vector2 playerToStairR2 = new(playerToStairR3.x, playerToStairR3.y);

        playerToStairR2.x = Mathf.Sign(playerToStairR2.x) * speed;
        playerToStairR2.y = 0;

        return playerToStairR2;
    }

    public Vector2 CalculateOnStairMovement(Vector2 userInput, float speed)
    {
        Vector2 directionVector;

        float dot = Vector2.Dot(userInput, m_dirToOtherStair);

        if (Mathf.Approximately(dot, 0f))
        {
            return Vector2.zero;
        }

        if (Vector2.Dot(userInput, m_dirToOtherStair) > 0)
        {
            directionVector = m_dirToOtherStair;
        }
        else
        {
            directionVector = -m_dirToOtherStair;
        }

        return directionVector * speed;
    }

    public bool CanEnterStair(Vector3 playerPosition)
    {
        Vector3 playerToStairEnter = transform.position - playerPosition;

        return playerToStairEnter.sqrMagnitude <= m_stairMovementThreshold * m_stairMovementThreshold;
    }

    public bool ShouldExitStair(Vector3 playerPosition)
    {
        Vector3 playerToStairEnter = m_destinationStair.transform.position - playerPosition;

        return playerToStairEnter.sqrMagnitude <= m_stairMovementThreshold * m_stairMovementThreshold;
    }

    public Vector2 ExitStairPos()
    {
        Vector3 destStairPos = m_destinationStair.transform.position;
        Vector2 desPos = new(destStairPos.x, destStairPos.y);

        return desPos;
    }

    public bool CanWalkToStair()
    {
        return m_player != null;
    }

    void OnEnterStair()
    {
        PlatformEffector2D upperCollider = m_upperCollider.GetComponent<PlatformEffector2D>();
        upperCollider.enabled = false;
    }

    void OnExitStair()
    {
        PlatformEffector2D upperCollider = m_upperCollider.GetComponent<PlatformEffector2D>();
        upperCollider.enabled = true;
    }

    // why are we even passing anything?
    void OnTriggerEnter2D(Collider2D other)
    {
        int layer = LayerMask.GetMask("Player");
        int shiftedObjectLayer = 1 << other.gameObject.layer;
        if ((shiftedObjectLayer & layer) != 0)
        {
            m_player = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        int layer = LayerMask.GetMask("Player");
        int shiftedObjectLayer = 1 << other.gameObject.layer;
        if ((shiftedObjectLayer & layer) != 0)
        {
            m_player = null;
        }
    }

    GameObject m_player;
    Vector2 m_dirToOtherStair;
}
