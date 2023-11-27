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
        m_checkEnableUpperCollider = false;
        m_upperColliderBox = m_upperCollider.GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        // allows for proper 1 way collisions
        // am not using platform collider since it can cause the player to collide inside of the platform
        if(m_checkEnableUpperCollider)
        {
            if (!IsPlayerInUpperCollider())
            {
                m_checkEnableUpperCollider = false;
                m_upperCollider.SetActive(true);
            }
        }
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

    public bool IsInputToEnterStair(Vector2 userInput)
    {
        userInput.x = 0f;

        return Vector2.Dot(m_dirToOtherStair, userInput) > 0.01f;
    }

    public bool ShouldExitStair(Vector3 playerPosition, Vector2 userInput)
    {
        Vector2 playerToStairEnter = transform.position - playerPosition;
        Vector2 otherStairToThisStair = transform.position - m_destinationStair.transform.position;

        // player is beyond the stair and should exit
        if(playerToStairEnter.sqrMagnitude > otherStairToThisStair.sqrMagnitude)
        {
            return true;
        }

        float userInputValue = Vector2.Dot(otherStairToThisStair, userInput);

        return userInputValue > 0.01f && playerToStairEnter.sqrMagnitude <= m_stairMovementThreshold * m_stairMovementThreshold;
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
        m_upperCollider.SetActive(false);
    }

    void OnExitStair()
    {
         m_upperCollider.SetActive(true);
    }

    void OnExitStairJump()
    {

        if (IsPlayerInUpperCollider())
        {
            m_checkEnableUpperCollider = true;
        }
        else
        {
            m_upperCollider.SetActive(true);
        }
    }

    bool IsPlayerInUpperCollider()
    {
        Vector2 point = m_upperColliderBox.offset + (Vector2) m_upperColliderBox.transform.position;
        int layerMask = LayerMask.NameToLayer("Player");
        Collider2D foundCollider = Physics2D.OverlapBox(point, m_upperColliderBox.size, 0f, 1 << layerMask);
        return foundCollider != null;
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

    void OnDrawGizmos()
    {
        if(m_upperColliderBox)
        {
            Gizmos.color = Color.yellow;
            Vector2 point = m_upperColliderBox.offset + (Vector2)m_upperColliderBox.transform.position;
            Gizmos.DrawCube(point, m_upperColliderBox.size);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, m_stairMovementThreshold);
    }

    GameObject m_player;
    BoxCollider2D m_upperColliderBox;
    Vector2 m_dirToOtherStair;
    bool m_checkEnableUpperCollider;
}
