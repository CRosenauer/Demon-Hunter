using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeSlider : Slider
{
    [SerializeField] GameObject m_menuPersistencyUpdator;
    [SerializeField] public MenuPersistency.MenuOption m_menuOption;
    [Space]

    [SerializeField] AudioMixer m_mixer;
    [SerializeField] Vector2 m_mixerGain;

    [SerializeField] AudioSource m_audioSource;
    [SerializeField] string m_audioMixerName;

    new void Awake()
    {
        Debug.Assert(m_mixer);
        Debug.Assert(m_menuPersistencyUpdator);
        base.Awake();
        OnSliderMove(false);
    }

    public override void OnSliderMove(bool playSound = true) 
    {
        float value = m_mixerGain.x + m_sliderValue * (m_mixerGain.y - m_mixerGain.x);

        // Mathf.Approximately doesnt work amazing. seems floating point error makes it too far from 0
        // instead we check if its decently below the next highest step
        if(m_sliderValue < (m_sliderStep * 0.5f))
        {
            value = -80; // dB. functionally mute audio
        }

        MenuPersistency.MenuField menuField = new();
        menuField.m_menuOption = m_menuOption;
        menuField.m_value = m_sliderValue;

        m_menuPersistencyUpdator.SendMessage("SetMenuField", menuField);

        m_mixer.SetFloat(m_audioMixerName, value);

        if(playSound && m_audioSource)
        {
            m_audioSource.Play();
        }
    }
}
