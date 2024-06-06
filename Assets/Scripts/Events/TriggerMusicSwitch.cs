using System.Collections;
using UnityEngine;

public class TriggerMusicSwitch : TriggerCallback
{
    [SerializeField] string m_objectName;
    [SerializeField] AudioClip m_audioClip;
    [SerializeField] float m_fadeOutScale;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    protected override void Callback()
    {
        StartCoroutine(VolumeReduceCoroutine());
    }

    IEnumerator VolumeReduceCoroutine()
    {
        GameObject systems = GameObject.Find(m_objectName);
        Debug.Assert(systems);
        AudioSource gameAudioPlayer = systems.GetComponent<AudioSource>();
        Debug.Assert(gameAudioPlayer);

        m_initialVolume = gameAudioPlayer.volume;

        do
        {
            yield return new WaitForFixedUpdate();
            gameAudioPlayer.volume -= m_fadeOutScale * Time.fixedDeltaTime;
        }
        while (gameAudioPlayer.volume > 0f);

        gameAudioPlayer.volume = m_initialVolume;
        gameAudioPlayer.clip = m_audioClip;
        gameAudioPlayer.Play();
    }

    float m_initialVolume;
}
