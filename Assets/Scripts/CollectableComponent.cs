using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CollectableComponent : MonoBehaviour
{
    [CustomEditor(typeof(CollectableComponent))]
    public class MyScriptEditor : Editor
    {
        SerializedProperty m_callback;
        SerializedProperty m_quantity;

        void OnEnable()
        {
            m_callback = serializedObject.FindProperty("m_callback");
            m_quantity = serializedObject.FindProperty("m_quantity");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_callback);

            CollectableComponent collectableComponent = target as CollectableComponent;
            if (collectableComponent.m_callback != CollectableFunction.Equip)
            {
                EditorGUILayout.PropertyField(m_quantity);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    enum CollectableFunction
    {
        Equip,
        AlterLife,
        AlterMagic,
        AlterScore,
    }

    delegate void CollectionCallback();

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
        m_callbackFunction();
        Destroy(gameObject);
    }

    void Equip()
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