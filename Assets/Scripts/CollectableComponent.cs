using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CollectableComponent : MonoBehaviour
{
#if UNITY_EDITOR
    [CustomEditor(typeof(CollectableComponent))]
    public class MyScriptEditor : Editor
    {
        SerializedProperty m_callback;
        SerializedProperty m_quantity;
        SerializedProperty m_item;

        void OnEnable()
        {
            m_callback = serializedObject.FindProperty("m_callback");
            m_quantity = serializedObject.FindProperty("m_quantity");
            m_item = serializedObject.FindProperty("m_item");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_callback);

            CollectableComponent collectableComponent = target as CollectableComponent;
            if (collectableComponent.m_callback != CollectableFunction.Equip)
            {
                if(collectableComponent.m_callback != CollectableFunction.ScreenClear)
                {
                    EditorGUILayout.PropertyField(m_quantity);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(m_item);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    enum CollectableFunction
    {
        Equip,
        ScreenClear,
        AlterLife,
        AlterMagic,
        AlterScore,
    }

    delegate void CollectionCallback();

    [SerializeField] GameObject m_item;
    [SerializeField] CollectableFunction m_callback;
    [SerializeField] int m_quantity;

    // Start is called before the first frame update
    void Start()
    {
        switch(m_callback)
        {
            case CollectableFunction.Equip:
                m_callbackFunction = Equip;
                break;
            case CollectableFunction.ScreenClear:
                m_callbackFunction = ScreenClear;
                break;
            case CollectableFunction.AlterLife:
                m_callbackFunction = AlterLife;
                break;
            case CollectableFunction.AlterMagic:
                m_callbackFunction = AlterMagic;
                break;
            case CollectableFunction.AlterScore:
                m_callbackFunction = AlterScore;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = LayerMask.GetMask("Player");
        int shiftedObjectLayer = 1 << collision.gameObject.layer;
        if ((shiftedObjectLayer & layer) == 0)
        {
            return;
        }

        m_callbackFunction();
        Destroy(gameObject);
    }

    void Equip()
    {

    }

    void ScreenClear()
    {

    }

    void AlterLife()
    {

    }

    void AlterMagic()
    {

    }

    void AlterScore()
    {

    }

    CollectionCallback m_callbackFunction;
}