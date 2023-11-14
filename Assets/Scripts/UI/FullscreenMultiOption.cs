using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullscreenMultiOption : Multioption<string>
{
    [SerializeField] ResolutionSettings m_resolutionSettings;

    void SetFullscreen()
    {
        switch(m_optionIndex)
        {
            case 0:
                m_resolutionSettings.SetFullscreen(FullScreenMode.Windowed);
                break;
            case 1:
                m_resolutionSettings.SetFullscreen(FullScreenMode.ExclusiveFullScreen);
                break;
        }
    }

    void UpdateText()
    {
        m_activeText.text = m_options[m_optionIndex];
    }

    public override void OnSliderMove(bool playSound = true)
    {
        SetFullscreen();
        UpdateText();
    }
}
