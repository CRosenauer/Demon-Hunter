using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Button : MonoBehaviour
{
    [SerializeField] protected Sprite m_selectedImage;

    protected void Start()
    {
        Debug.Assert(m_selectedImage);
    }

    void Awake()
    {
        
    }

    void OnHover()
    {
        
    }

    void OnUnHover()
    {

    }

    public abstract void OnSelect();
}
