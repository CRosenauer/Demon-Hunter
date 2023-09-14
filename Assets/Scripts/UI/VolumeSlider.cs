using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeSlider : Slider
{
    [SerializeField] AudioMixer m_mixer;
    [SerializeField] Vector2 m_mixerGain;

    [SerializeField] AudioSource m_audioSource;
    [SerializeField] string m_audioMixerName;

    new void Awake()
    {
        Debug.Assert(m_mixer);
        base.Awake();
        OnSliderMove();
    }

    public override void OnSliderMove() 
    {
        float value = m_mixerGain.x + m_sliderValue * (m_mixerGain.y - m_mixerGain.x);

        m_mixer.SetFloat(m_audioMixerName, value);

        if(m_audioSource)
        {
            m_audioSource.Play();
        }
    }
}
