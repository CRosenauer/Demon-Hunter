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

        if(!PlayerPrefs.HasKey(MenuOption.sound.ToString()))
        {
            PlayerPrefs.SetFloat(MenuOption.sound.ToString(), 0.8f);
        }

        if (!PlayerPrefs.HasKey(MenuOption.music.ToString()))
        {
            PlayerPrefs.SetFloat(MenuOption.music.ToString(), 0.8f);
        }

        PlayerPrefs.Save();

        RestoreMenuOptions();
    }

    void RestoreMenuOptions()
    {
        m_sound.GetComponent<Slider>()?.ForceSetSliderValue(PlayerPrefs.GetFloat(MenuOption.sound.ToString(), 0.8f));
        m_music.GetComponent<Slider>()?.ForceSetSliderValue(PlayerPrefs.GetFloat(MenuOption.music.ToString(), 0.8f));
    }

    void SetMenuField(MenuField menuField)
    {
        if(m_menuOptions == null)
        {
            return;
        }

        PlayerPrefs.SetFloat(menuField.m_menuOption.ToString(), menuField.m_value);
        PlayerPrefs.Save();
    }

    Dictionary<MenuOption, float> m_menuOptions;
}
