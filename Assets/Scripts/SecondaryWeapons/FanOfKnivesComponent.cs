using System.Collections.Generic;
using UnityEngine;

public class FanOfKnivesComponent : SecondaryWeaponComponent
{
    [SerializeField] GameObject m_knifeObject;
    [SerializeField] List<float> m_kniveAngles;

    public override void OnSpawn(float direction)
    {
        foreach (float angle in m_kniveAngles)
        {
            float correctedAngle = direction > 0f ? angle : 180f - angle;

            Quaternion orientation = Quaternion.AngleAxis(correctedAngle, Vector3.forward);
            Instantiate(m_knifeObject, transform.position, orientation, transform.parent);
        }

        Destroy(gameObject);
    }
}
