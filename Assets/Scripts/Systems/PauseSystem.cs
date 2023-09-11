using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseSystem : MonoBehaviour
{
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
            SetFreezeAll(!m_frozen);
        }

        m_pauseDownLastUpdate = pauseDown;
    }

    void SetFreezeAll(bool freeze)
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

    }

    void OnUnPause()
    {

    }

    void SignalPauseMenu(bool open)
    {

    }

    bool m_frozen;
    bool m_pauseDownLastUpdate;
}
