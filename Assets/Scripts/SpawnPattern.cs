using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RetroActionPlatformer/SpawnPattern")]
public class SpawnPattern : ScriptableObject
{
    [SerializeField] public List<Vector3> m_positionOffsets;
}