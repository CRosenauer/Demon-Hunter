using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanOfKnivesComponent : SecondaryWeaponComponent
{
    [SerializeField] GameObject m_knifeObject;
    [SerializeField] List<float> m_kniveAngles;

    void Update()
    {
        /* debug code
        if(Input.GetButton("Special"))
        {
            PlayerMovement playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            OnSpawn(playerMovement.GetDirection());
        }
        */
    }

    public override void OnSpawn(MovementComponent.Direction direction)
    {
        foreach (float angle in m_kniveAngles)
        {
            float correctedAngle = direction == MovementComponent.Direction.left ? 180f - angle : angle;

            Quaternion orientation = Quaternion.AngleAxis(correctedAngle, Vector3.forward);
            Instantiate(m_knifeObject, transform.position, orientation, transform.parent);
        }

        Destroy(gameObject);
    }
}
