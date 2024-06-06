using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemBoxComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Assert(player);

        SecondaryWeaponManagerComponent secondaryWeapon = player.GetComponent<SecondaryWeaponManagerComponent>();
        Debug.Assert(secondaryWeapon);

        secondaryWeapon.OnWeaponChanged += OnWeaponChanged;

        OnWeaponChanged(null);
    }

    void OnWeaponChanged(Sprite weaponImage)
    {
        Image image = GetComponent<Image>();

        if(weaponImage)
        {
            image.enabled = true;
            image.sprite = weaponImage;
        }
        else
        {
            image.enabled = false;
        }
    }
}
