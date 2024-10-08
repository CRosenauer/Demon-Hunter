using UnityEngine;

public abstract class Slider : MonoBehaviour
{
    // public to give access to child class
    [SerializeField] public GameObject m_sliderKnob;

    [SerializeField] Vector2 m_sliderPositionRange;
    [SerializeField] Vector2 m_sliderValueRange;
    [SerializeField] protected float m_sliderStep;

    protected void Awake()
    {
        Debug.Assert(m_sliderKnob);
    }

    protected void SetSliderPosition()
    {
        float sliderRange = m_sliderPositionRange.y - m_sliderPositionRange.x;
        float sliderPercentage = m_sliderValue / (m_sliderValueRange.y - m_sliderValueRange.x);
        float xPos = m_sliderPositionRange.x + sliderPercentage * sliderRange;

        RectTransform sliderTransform = m_sliderKnob.GetComponent<RectTransform>();
        Vector3 sliderPos = sliderTransform.anchoredPosition;
        sliderPos.x = xPos;

        sliderTransform.anchoredPosition = sliderPos;
    }

    protected void MoveSlider(float amount)
    {
        m_sliderValue = m_sliderValue + m_sliderStep * Mathf.Sign(amount);

        ClampSliderValue();

        SetSliderPosition();

        OnSliderMove();
    }

    public void ForceSetSliderValue(float value)
    {
        value = Mathf.Clamp01(value);
        m_sliderValue = Mathf.Lerp(m_sliderValueRange.x, m_sliderValueRange.y, value);
        SetSliderPosition();
        OnSliderMove(false);
    }

    protected void ClampSliderValue()
    {
        m_sliderValue = Mathf.Clamp(m_sliderValue, m_sliderValueRange.x, m_sliderValueRange.y);
    }

    public abstract void OnSliderMove(bool playSound = true);

    protected float m_sliderValue;
}
