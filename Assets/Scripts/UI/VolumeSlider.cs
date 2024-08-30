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

    [SerializeField] string m_prefsName;

    new void Awake()
    {
        Debug.Assert(m_mixer);
        Debug.Assert(m_menuPersistencyUpdator);
        base.Awake();

        m_sliderValue = PlayerPrefs.GetFloat(m_prefsName, 0.8f);
        ClampSliderValue();
        SetSliderPosition();
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

        // Todo: this should really be on a base class
        // it doesn't make sense for this shared logic to not be shared between every persistent/preference UI element
        // could maybe pass a scriptable object for storing option keys or if the value should be persistent.
        PlayerPrefs.SetFloat(m_prefsName, m_sliderValue);
        PlayerPrefs.Save();

        m_mixer.SetFloat(m_audioMixerName, value);

        if(playSound && m_audioSource)
        {
            m_audioSource.Play();
        }
    }
}
