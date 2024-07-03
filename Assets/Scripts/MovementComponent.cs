using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
public class MovementComponent : MonoBehaviour
{
    [SerializeField]
    private PhysicsCollision m_physicsCollisionLayers;

    private const float k_skinWidth = 0.01f;

    public Vector2 Velocity => m_velocity;

    public bool IsOnGround => m_isOnGround;
    public bool CanMove { set { m_canMove = value; } get { return m_canMove; } }
    public bool IsInCutscene { set; get; }

    [SerializeField]
    private bool m_applyGravity = true;
    public bool ApplyGravity { set { m_applyGravity = value; } get { return m_applyGravity; } }

    public Animator Animator => m_animator;

    private struct BoxPhysicsDescriptor
    {
        public Vector2 m_center;
        public Vector2 m_extents;
    }

    public delegate void CollisionEventHandler(Collider2D other);
    
    private event CollisionEventHandler m_collisionEventHandler;

    public void AddOnCollision(CollisionEventHandler collisionCallback)
    {
        m_collisionEventHandler += collisionCallback;
    }

    public void RemoveOnCollision(CollisionEventHandler collisionCallback)
    {
        m_collisionEventHandler -= collisionCallback;
    }

    public void UpdateDirection(float direction)
    {
        if (Mathf.Approximately(direction, 0f))
        {
            return;
        }

        direction = Mathf.Sign(direction);

        Vector3 scale = transform.localScale;
        scale.x = direction;
        transform.localScale = scale;
    }

    public void Move(Vector2 velocity)
    {
        m_velocity = velocity;
    }

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_collider = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        if(!CanMove)
        {
            return;
        }

        if(m_isInCutscene)
        {
            return;
        }


        Vector2 velocity = m_velocity;

        if (ApplyGravity)
        {
            velocity += Physics2D.gravity * Time.fixedDeltaTime;
        }

        Vector2 requestedMovement = velocity * Time.fixedDeltaTime;

        Vector2 requestedHorizontalMovement = Vector2.right * requestedMovement.x;

        int layerMask = m_physicsCollisionLayers == null ? 0 : m_physicsCollisionLayers.CollisionLayerMask;
        RaycastHit2D result;

        Vector2 horizontalMovement = CollideAndSlide(requestedHorizontalMovement, layerMask, out result);
        transform.position += (Vector3) horizontalMovement;

        Collider2D collidedCollider = result.collider;

        Vector2 requestedVerticalMovement = Vector2.up * requestedMovement.y;
        Vector2 verticalMovement = CollideAndSlide(requestedVerticalMovement, layerMask, out result);
        transform.position += (Vector3) verticalMovement;

        if(collidedCollider == null)
        {
            collidedCollider = result.collider;
        }

        Vector2 totalMovement = horizontalMovement + verticalMovement;

        m_isOnGround = verticalMovement.y > requestedMovement.y;
        m_velocity = totalMovement / Time.fixedDeltaTime;

        if (!m_isInCutscene)
        {
            m_animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        }

        if(collidedCollider != null)
        {
            m_collisionEventHandler?.Invoke(collidedCollider);
        }
    }

    private Vector2 CollideAndSlide(Vector2 movement, int layerMask, out RaycastHit2D result)
    {
        GetBoxPositionDescriptor(m_collider, out BoxPhysicsDescriptor descriptor);
        return CollideAndSlide_Internal(descriptor, movement, layerMask, out result);
    }

    private Vector2 CollideAndSlide_Internal(BoxPhysicsDescriptor descriptor, Vector2 movement, int layerMask, out RaycastHit2D result)
    {
        Vector2 direction = movement.normalized;
        float distance = movement.magnitude + k_skinWidth;

        result = Physics2D.BoxCast(descriptor.m_center, descriptor.m_extents, 0f, direction, distance, layerMask);

        if(result.collider == null)
        {
            return movement;
        }

        Vector2 snapToSurface = direction * (result.distance - k_skinWidth);

        if(snapToSurface.magnitude <= k_skinWidth)
        {
            return Vector2.zero;
        }

        return snapToSurface;
    }

    private static void GetBoxPositionDescriptor(BoxCollider2D boxCollider, out BoxPhysicsDescriptor boxPhysicsDescriptor)
    {
        boxPhysicsDescriptor = new();

        if (!boxCollider)
        {
            return;
        }

        boxPhysicsDescriptor.m_center = (Vector2) boxCollider.transform.position + boxCollider.offset;
        boxPhysicsDescriptor.m_extents.x = boxCollider.size.x;
        boxPhysicsDescriptor.m_extents.y = boxCollider.size.y;
    }

    private Animator m_animator;

    private BoxCollider2D m_collider;

    private Vector2 m_velocity;

    private bool m_isOnGround = true;
    private bool m_isInCutscene = false;

    private bool m_canMove = true;
}
