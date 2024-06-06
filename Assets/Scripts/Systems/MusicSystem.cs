using UnityEngine;

public class MusicSystem : MonoBehaviour
{
    [SerializeField] AudioSource m_backgroundMusicSource;

    void ToggleMusic(bool play)
    {
        if(play)
        {
            m_backgroundMusicSource.Play();
        }
        else
        {
            m_backgroundMusicSource.Pause();
        }
    }
}
