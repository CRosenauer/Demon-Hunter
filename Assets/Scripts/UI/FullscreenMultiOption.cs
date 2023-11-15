using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullscreenMultiOption : MultiOption<string>
{
    [SerializeField] ResolutionSettings m_resolutionSettings;
    [Space]

    [SerializeField] GameObject m_menuPersistencyUpdator;
    [SerializeField] public MenuPersistency.MenuOption m_menuOption;

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

    protected override void UpdateText()
    {
        m_activeText.text = m_options[m_optionIndex];
    }

    public override void OnSliderMove(bool playSound = true)
    {
        MenuPersistency.MenuField menuField = new();
        menuField.m_menuOption = m_menuOption;
        menuField.m_value = m_optionIndex;

        m_menuPersistencyUpdator.SendMessage("SetMenuField", menuField);

        SetFullscreen();
        UpdateText();
    }
}
