using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SecondaryWeaponComponent : MonoBehaviour
{
    public abstract void OnSpawn(MovementComponent.Direction direction);
}
