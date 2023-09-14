using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPageController : MonoBehaviour
{
    [SerializeField] GameObject m_defaultMenuPage;

    // Start is called before the first frame update
    void Start()
    {
        m_menuPage = m_defaultMenuPage;
        m_menuPage.SetActive(true);
    }

    void OpenMenuPage(GameObject menuPage)
    {
        m_menuPage.SetActive(false);
        m_menuPage = menuPage;
        m_menuPage.SetActive(true);
        m_menuPage.SendMessage("ResetCursorPosition");
    }

    void OnPause()
    {
        OpenMenuPage(m_defaultMenuPage);
    }

    GameObject m_menuPage;
}
