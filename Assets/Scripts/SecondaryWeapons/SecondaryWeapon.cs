using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RetroActionPlatformer/SecondaryWeapon")]
public class SecondaryWeapon : ScriptableObject
{
    public Sprite m_HUDIcon;
    [Space]

    public GameObject m_spawnedObject;
    [Space]

    public string m_animationTrigger;

    public float m_startUpDuration;
    public float m_timeInUsageDuration;

    public int m_manaCost;
}