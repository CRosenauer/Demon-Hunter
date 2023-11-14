using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    void TogglePause()
    {
        SetPause(!m_frozen);
    }

    void SetPause(bool freeze)
    {
        m_frozen = freeze;

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

        MenuPageController menuPageController = m_menus.GetComponent<MenuPageController>();
        menuPageController.enabled = true;

        Image pauseBackground = m_menus.GetComponent<Image>();
        pauseBackground.enabled = true;

        m_menus.SendMessage("OnPause");
    }

    void OnUnPause()
    {
        if (m_backgroundMusicSource)
        {
            m_backgroundMusicSource.UnPause();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            player.BroadcastMessage("OnUnpause");
        }

        MenuPageController menuPageController = m_menus.GetComponent<MenuPageController>();
        menuPageController.enabled = false;

        Image pauseBackground = m_menus.GetComponent<Image>();
        pauseBackground.enabled = false;

        m_menus.SendMessage("OnUnpause");
    }

    bool m_frozen;
    bool m_pauseDownLastUpdate;
}
