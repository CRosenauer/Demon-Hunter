using UnityEngine;

public class ScoreReseter : MonoBehaviour
{
    [SerializeField] ScoreChangedEvent m_scoreChangedEvent;

    void Awake()
    {
        m_scoreChangedEvent.Reset();
    }
}
