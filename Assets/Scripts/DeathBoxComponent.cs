using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBoxComponent : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // bit hacky. used to update the health UI
        collision.BroadcastMessage("Heal", -1000);

        collision.BroadcastMessage("ForceDeath");
    }
}
