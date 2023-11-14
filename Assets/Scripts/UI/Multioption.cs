using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Multioption<T> : MonoBehaviour
{
    [SerializeField] GameObject m_rightArrow;
    [SerializeField] GameObject m_leftArrow;
    [SerializeField] protected List<T> m_options;
    [SerializeField] protected Text m_activeText;

    protected void MoveSlider(float amount)
    {
        m_optionIndex = m_optionIndex + (int) Mathf.Sign(amount);

        ClampOptionsIndex();

        OnSliderMove();
    }

    void ClampOptionsIndex()
    {
        m_optionIndex = Mathf.Clamp(m_optionIndex, 0, m_options.Count - 1);

        if(m_optionIndex == 0)
        {
            m_leftArrow.SetActive(false);
        }
        else
        {
            m_leftArrow.SetActive(true);
        }

        if (m_optionIndex == m_options.Count - 1)
        {
            m_rightArrow.SetActive(false);
        }
        else
        {
            m_rightArrow.SetActive(true);
        }
    }

    public abstract void OnSliderMove(bool playSound = true);

    protected int m_optionIndex;
}
