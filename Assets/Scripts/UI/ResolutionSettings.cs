using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Resolution")]
public class ResolutionSettings : ScriptableObject
{
    public void SetResolution(Vector2Int res)
    {
        m_resolution = res;
        ApplyResolutionSettings();
    }

    public void SetFullscreen(FullScreenMode fs)
    {
        m_fullscreen = fs;
        ApplyResolutionSettings();
    }

    void ApplyResolutionSettings()
    {
        Screen.SetResolution(m_resolution.x, m_resolution.y, FullScreenMode.Windowed);
    }

    Vector2Int m_resolution;
    FullScreenMode m_fullscreen;
}
