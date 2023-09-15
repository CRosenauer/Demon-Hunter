using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "RetroActionPlatformer/AttackData")]
public class SeconaryWeapon : ScriptableObject
{
    public Image m_HUDIcon;
    [Space]

    public GameObject m_spawnedObject;
    [Space]

    public float m_startUpDuration;
    public float m_timeInUsageDuration;
    public string m_animationTrigger;
}