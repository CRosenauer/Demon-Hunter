using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionMultiOption : MultiOption<Vector2Int>
{
    [SerializeField] ResolutionSettings m_resolutionSettings;
    [Space]

    [SerializeField] GameObject m_menuPersistencyUpdator;
    [SerializeField] public MenuPersistency.MenuOption m_menuOption;

    void Start()
    {
        InitOptionText();
    }

    void InitOptionText()
    {
        if(m_optionText != null)
        {
            return;
        }

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

    protected override void UpdateText()
    {
        InitOptionText();
        m_activeText.text = m_optionText[m_optionIndex];
    }

    public override void OnSliderMove(bool playSound = true)
    {
        MenuPersistency.MenuField menuField = new();
        menuField.m_menuOption = m_menuOption;
        menuField.m_value = m_optionIndex;

        m_menuPersistencyUpdator.SendMessage("SetMenuField", menuField);

        SetResolution();
        UpdateText();
    }

    List<string> m_optionText;
}
