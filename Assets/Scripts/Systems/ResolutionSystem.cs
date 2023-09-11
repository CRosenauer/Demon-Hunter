using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(854, 480, true, 60);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
