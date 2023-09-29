using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffScreenFreezeComponent : FreezeComponent
{
    void Start()
    {
        Freeze();
    }

    void OnEnterCamera()
    {
        Unfreeze();
    }

    void OnExitCamera()
    {
        Freeze();
    }
}
