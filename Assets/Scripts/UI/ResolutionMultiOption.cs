using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionMultiOption : Multioption<Vector2Int>
{
    [SerializeField] ResolutionSettings m_resolutionSettings;

    void Start()
    {
        m_optionText = new();

        foreach (Vector2Int resolution in m_options)
        {
            m_optionText.Add($"{resolution.x} x {resolution.y}");
        }
    }

    void SetResolution()
    {
        m_resolutionSettings.SetResolution(m_options[m_optionIndex]);
    }

    void UpdateText()
    {
        m_activeText.text = m_optionText[m_optionIndex];
    }

    public override void OnSliderMove(bool playSound = true)
    {
        SetResolution();
        UpdateText();
    }

    List<string> m_optionText;
}
