using UnityEngine;

[CreateAssetMenu(menuName = "RetroActionPlatformer/PhysicsCollisionLayers")]
public class PhysicsCollision : ScriptableObject
{
    [SerializeField]
    private LayerMask m_collisionLayerMask;

    public LayerMask CollisionLayerMask => m_collisionLayerMask;
}
