
public class OffScreenFreezeComponent : FreezeComponent
{
    void Start()
    {
        Freeze();
    }

    void OnEnterCamera()
    {
        Unfreeze();
    }

    void OnExitCamera()
    {
        Freeze();
    }
}
