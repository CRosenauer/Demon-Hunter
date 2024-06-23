using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LivesCounter : MonoBehaviour
{
    public void OnLivesChanged(int lives)
    {
        Text text = GetComponent<Text>();
        text.text = $"x{lives}";
    }
}
