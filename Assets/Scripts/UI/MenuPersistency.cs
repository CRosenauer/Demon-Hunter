using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class MenuPersistency : MonoBehaviour
{
    [SerializeField] string m_path;
    [SerializeField] GameObject m_music;
    [SerializeField] GameObject m_sound;

    public enum MenuOption
    {
        sound,
        music,
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

        if (!RestoreMenuOptions())
        {
            CreateDefaultMenuOptions();
        }
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
            }
        }

        return true;
    }

    void CreateDefaultMenuOptions()
    {
        string defaults = "music:0.8\nsound:0.8";

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
