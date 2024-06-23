using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScoreHUD : MonoBehaviour
{
    public void SetScoreText(int score)
    {
        Text text = GetComponent<Text>();
        text.text = $"{score}";
    }
}
