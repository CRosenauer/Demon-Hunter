using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuNavigator : MonoBehaviour
{
    [SerializeField] List<GameObject> m_menuItems;
    [SerializeField] GameObject m_menuPointer;

    [SerializeField] GameObject m_systemManager;

    [SerializeField] AudioSource m_menuSoundSource;

    const float k_axisThreshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {

        m_horizontalLastFrame = 0f;
        m_verticalLastFrame = 0f;
        m_submitLastFrame = false;
        m_cancelLastFrame = false;

        m_cursorPosition = 0;
        m_maxCursorPosistion = m_menuItems.Count;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool submit = Input.GetButton("Submit");
        bool cancel = Input.GetButton("Cancel");

        if(Mathf.Abs(m_verticalLastFrame) <= k_axisThreshold && Mathf.Abs(verticalInput) > k_axisThreshold)
        {
            MoveCursor((int) -Mathf.Sign(verticalInput));
        }

        if (Mathf.Abs(m_horizontalLastFrame) <= k_axisThreshold && Mathf.Abs(horizontalInput) > k_axisThreshold)
        {
            MoveSlider((int) Mathf.Sign(horizontalInput));
        }

        if (submit && !m_submitLastFrame)
        {
            SelectMenuItem();
        }

        if (cancel && !m_cancelLastFrame)
        {
            OnQuit();
        }

        m_horizontalLastFrame = horizontalInput;
        m_verticalLastFrame = verticalInput;
        m_submitLastFrame = submit;
        m_cancelLastFrame = cancel;
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

    void SelectMenuItem()
    {
        m_menuItems[m_cursorPosition].BroadcastMessage("OnSelect");
    }

    void UpdateCursorPosition()
    {
        Vector3 itemPosition = m_menuItems[m_cursorPosition].transform.position;
        Vector3 cursorPosition = m_menuPointer.transform.position;
        cursorPosition.y = itemPosition.y;

        m_menuPointer.transform.position = cursorPosition;
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
