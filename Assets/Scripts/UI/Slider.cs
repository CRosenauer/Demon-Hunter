using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Slider : MonoBehaviour
{
    [SerializeField] GameObject m_unSelectedImage;
    [SerializeField] GameObject m_selectedImage;

    [SerializeField] GameObject m_sliderKnob;

    [SerializeField] Vector2 m_sliderRange;
    [SerializeField, Range(0f, 1f)] float m_defaultSliderValue;

    void Start()
    {
        Debug.Assert(m_unSelectedImage);
        Debug.Assert(m_selectedImage);
        Debug.Assert(m_sliderKnob);
    }

    void Awake()
    {
        m_sliderValue = m_defaultSliderValue;

        SetSliderPosition(m_sliderValue);
    }

    void SetSliderPosition(float sliderValue)
    {
        float sliderRange = m_sliderRange.y - m_sliderRange.x;
        float xPos = m_sliderRange.x + sliderValue * sliderRange;

        Vector3 sliderPos = m_sliderKnob.transform.position;
        sliderPos.x = xPos;

        m_sliderKnob.transform.position = sliderPos;
    }

    void OnHover()
    {
        m_selectedImage.active = true;
        m_unSelectedImage.active = false;
    }

    void OnUnHover()
    {
        m_unSelectedImage.active = true;
        m_selectedImage.active = false;
    }

    void MoveSlider(float sliderValueDelta)
    {
        m_sliderValue = m_sliderValue + sliderValueDelta;

        m_sliderValue = Mathf.Clamp01(m_sliderValue);

        SetSliderPosition(m_sliderValue);

        OnSliderMove();
    }

    public abstract void OnSliderMove();

    float m_sliderValue;
}
