using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseSystem : MonoBehaviour
{
    [SerializeField] AudioSource m_backgroundMusicSource;
    [SerializeField] AudioSource m_pauseSoundSource;

    [SerializeField] GameObject m_menus;

    // Start is called before the first frame update
    void Start()
    {
        m_frozen = false;
        m_pauseDownLastUpdate = false;
    }

    // Update is called once per frame
    void Update()
    {
        bool pauseDown = Input.GetButton("Pause");

        if(pauseDown && !m_pauseDownLastUpdate)
        {
            SetPause(!m_frozen);
        }

        m_pauseDownLastUpdate = pauseDown;
    }

    void SetPause(bool freeze)
    {
        m_frozen = freeze;
        SignalPauseMenu(m_frozen);

        if(m_frozen)
        {
            OnPause();
            Time.timeScale = 0f;
        }
        else
        {
            OnUnPause();
            Time.timeScale = 1f;
        }
    }

    void OnPause()
    {
        if(m_backgroundMusicSource)
        {
            m_backgroundMusicSource.Pause();
        }

        if(m_pauseSoundSource)
        {
            m_pauseSoundSource.Play();
        }

        if(m_menus)
        {
            m_menus.active = true;
        }
    }

    void OnUnPause()
    {
        if (m_backgroundMusicSource)
        {
            m_backgroundMusicSource.UnPause();
        }

        if (m_menus)
        {
            m_menus.active = false;
        }
    }

    void SignalPauseMenu(bool open)
    {

    }

    bool m_frozen;
    bool m_pauseDownLastUpdate;
}
