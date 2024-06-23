using UnityEngine;

public class MenuNavitagionButton : Button
{
    [SerializeField] GameObject m_targetObject;
    [SerializeField] GameObject m_destinationMenuPage;

    public override void OnSelect()
    {
        m_targetObject.BroadcastMessage("OpenMenuPage", m_destinationMenuPage);
    }
}
