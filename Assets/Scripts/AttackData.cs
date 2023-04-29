using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RetroActionPlatformer/AttackData")]
public class AttackData : ScriptableObject
{
    [SerializeField] public Vector2 m_collisionBounds;
    [SerializeField] public Vector2 m_collisionOffset;
    [Space]

    [SerializeField] public int m_damage;

    [SerializeField] public int m_startUpFrames;
    [SerializeField] public int m_activeFrames;
    [SerializeField] public int m_recoveryFrames;
    [Space]

    [SerializeField] public string m_animationTrigger;
    [Space]

    [SerializeField] public AttackData m_nextAttack;
    [SerializeField] public int m_interruptableAsSoonAs;
}

