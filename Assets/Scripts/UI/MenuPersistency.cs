using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class MenuPersistency : MonoBehaviour
{
    [SerializeField] string m_path;
    [SerializeField] GameObject m_music;
    [SerializeField] GameObject m_sound;
    [SerializeField] GameObject m_resolution;
    // [SerializeField] GameObject m_fullscreen;

    public enum MenuOption
    {
        sound,
        music,
        resolution,
        fullscreen
    }

    public struct MenuField
    {
        public MenuOption m_menuOption;
        public float m_value;
    }

    void Start()
    {
        m_menuOptions = new();

        m_menuOptions.Add(MenuOption.sound, 0f);
        m_menuOptions.Add(MenuOption.music, 0f);
        m_menuOptions.Add(MenuOption.resolution, 0f);
        m_menuOptions.Add(MenuOption.fullscreen, 0f);

        if (!RestoreMenuOptions())
        {
            CreateDefaultMenuOptions();
        }

        ApplyOptions();
    }

    void ApplyOptions()
    {
        VolumeSlider musicslider = m_music.GetComponent<VolumeSlider>();
        musicslider.ForceSetSliderValue(m_menuOptions[MenuOption.music]);
        VolumeSlider soundslider = m_sound.GetComponent<VolumeSlider>();
        soundslider.ForceSetSliderValue(m_menuOptions[MenuOption.sound]);

        ResolutionMultiOption resolution = m_resolution.GetComponent<ResolutionMultiOption>();
        resolution.ForceOptionUpdate(Mathf.RoundToInt(m_menuOptions[MenuOption.resolution]));
        resolution.CullInvalidResolutions();

        // fullscreen not supported currently
        // MultiOption<int> fullscreen = m_fullscreen.GetComponent<MultiOption<int>>();
        // fullscreen.ForceOptionUpdate(Mathf.RoundToInt(m_menuOptions[MenuOption.fullscreen]));
    }

    bool RestoreMenuOptions()
    {
        if(!File.Exists(m_path))
        {
            return false;
        }

        StreamReader reader = new StreamReader(m_path);

        string text = reader.ReadToEnd();
        reader.Close();
        string[] options = text.Split('\n');

        foreach(string str in options)
        {
            string[] optionFields = str.Split(':');

            MenuOption option;

            string optionStr = optionFields[0];

            if(optionStr == "sound")
            {
                option = MenuOption.sound;
            }
            else if(optionStr == "music")
            {
                option = MenuOption.music;
            }
            else if(optionStr == "resolution")
            {
                option = MenuOption.resolution;
            }
            else if (optionStr == "fullscreen")
            {
                option = MenuOption.fullscreen;
            }
            else
            {
                continue;
            }

            float value = (float) Convert.ToDouble(optionFields[1]);

            m_menuOptions[option] = value;
        }

        List<MenuOption> keys = new();

        foreach(MenuOption key in m_menuOptions.Keys)
        {
            keys.Add(key);
        }

        foreach(MenuOption key in keys)
        {
            switch(key)
            {
                case MenuOption.sound:
                    m_sound.SendMessage("ForceSetSliderValue", m_menuOptions[key]);
                    break;
                case MenuOption.music:
                    m_music.SendMessage("ForceSetSliderValue", m_menuOptions[key]);
                    break;
                case MenuOption.resolution:
                    m_resolution.SendMessage("ForceOptionUpdate", m_menuOptions[key]);
                    break;
                case MenuOption.fullscreen:
                    // m_fullscreen.SendMessage("ForceOptionUpdate", m_menuOptions[key]);
                    break;
            }
        }

        return true;
    }

    void CreateDefaultMenuOptions()
    {
        string defaults = "music:0.8\nsound:0.8\nresolution:0\nfullscreen:0";

        StreamWriter writer = new(m_path);
        writer.Write(defaults);
        writer.Close();
        RestoreMenuOptions();
    }

    void SaveMenuOptions()
    {
        string settings = "music:";
        string musicValue = Convert.ToString(m_menuOptions[MenuOption.music]);
        settings = settings + musicValue;
        settings = settings + "\nsound:";
        string soundValue = Convert.ToString(m_menuOptions[MenuOption.sound]);
        settings = settings + soundValue;
        settings = settings + "\nresolution:";
        string resolutionIndex = Convert.ToString(m_menuOptions[MenuOption.resolution]);
        settings = settings + resolutionIndex;
        settings = settings + "\nfullscreen:";
        string fullscreenIndex = Convert.ToString(m_menuOptions[MenuOption.fullscreen]);
        settings = settings + fullscreenIndex;

        StreamWriter writer = new(m_path);
        writer.Write(settings);
        writer.Close();
    }

    void SetMenuField(MenuField menuField)
    {
        if(m_menuOptions == null)
        {
            return;
        }

        m_menuOptions[menuField.m_menuOption] = menuField.m_value;
        SaveMenuOptions();
    }

    Dictionary<MenuOption, float> m_menuOptions;
}
