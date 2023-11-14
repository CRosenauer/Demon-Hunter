using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuNavigator : MonoBehaviour
{
    [SerializeField] GameObject m_menuPages;

    [SerializeField] List<GameObject> m_menuItems;
    [SerializeField] GameObject m_menuPointer;

    [SerializeField] GameObject m_systemManager;

    [SerializeField] AudioSource m_menuSoundSource;

    [SerializeField] GameObject m_returnMenu;

    const float k_axisThreshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {

        m_horizontalLastFrame = 0f;
        m_verticalLastFrame = 0f;
        m_submitLastFrame = true;
        m_cancelLastFrame = true;

        ResetCursorPosition();
        m_maxCursorPosistion = m_menuItems.Count;
    }

    void OnEnable()
    {
        m_submitLastFrame = true;
        m_cancelLastFrame = true;
    }

    public void MoveMenuCursor(Vector2 input)
    {
        if(input.y != 0f)
        {
            MoveCursor((int)-Mathf.Sign(input.y));
        }

        if (input.x != 0f)
        {
            MoveSlider((int)Mathf.Sign(input.x));
        }
    }

    public void SelectMenuItem()
    {
        m_menuItems[m_cursorPosition].BroadcastMessage("OnSelect");
    }

    public void OnReturnMenu()
    {
        if (m_returnMenu)
        {
            m_menuPages.SendMessage("OpenMenuPage", m_returnMenu);
        }
        else
        {
            if (m_systemManager)
            {
                m_systemManager.SendMessage("TogglePause");
            }
        }
    }

    void MoveCursor(int amount)
    {
        m_cursorPosition = m_cursorPosition + amount;

        m_cursorPosition = Mathf.Clamp(m_cursorPosition, 0, m_maxCursorPosistion - 1);

        if(m_menuSoundSource)
        {
            m_menuSoundSource.Play();
        }

        UpdateCursorPosition();
    }

    void MoveSlider(int amount)
    {
        m_menuItems[m_cursorPosition].BroadcastMessage("MoveSlider", amount);
    }

    void UpdateCursorPosition()
    {
        Vector3 itemPosition = m_menuItems[m_cursorPosition].transform.position;
        Vector3 cursorPosition = m_menuPointer.transform.position;
        cursorPosition.y = itemPosition.y;

        m_menuPointer.transform.position = cursorPosition;
    }

    public void ResetCursorPosition()
    {
        m_cursorPosition = 0;
        UpdateCursorPosition();
    }

    void OnQuit()
    {
        Application.Quit();
    }

    float m_horizontalLastFrame;
    float m_verticalLastFrame;

    int m_cursorPosition;
    int m_maxCursorPosistion;

    bool m_submitLastFrame;
    bool m_cancelLastFrame;
}
