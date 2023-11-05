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
        SerializedProperty m_audioSource;

        void OnEnable()
        {
            m_audioSource = serializedObject.FindProperty("m_audioSource");
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
                EditorGUILayout.PropertyField(m_audioSource);
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

    [SerializeField] AudioSource m_audioSource;
    [SerializeField] SecondaryWeapon m_item;
    [SerializeField] CollectableFunction m_callback;
    [SerializeField] int m_quantity;

    // Start is called before the first frame update
    void Start()
    {
        InitCallback();
    }

    void InitCallback()
    {
        switch (m_callback)
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

        // if something is overlapping the item as it spawns we can reach this function before start inits the callback.
        if(m_callbackFunction == null)
        {
            InitCallback();
        }

        m_callbackFunction();
    }

    IEnumerator DestroyCallback()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;

        Collider2D[] colliders = GetComponents<Collider2D>();
        
        // only want to disable the trigger for picking up this item
        foreach(Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        yield return new WaitForSeconds(m_audioSource.clip.length);
        Destroy(gameObject);
    }

    void Equip()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        Debug.Assert(player);

        player.BroadcastMessage("SetSecondaryWeapon", m_item);
        Destroy(gameObject);
    }

    void ScreenClear()
    {
        StartCoroutine(BookComponent.ScreenClear(m_audioSource, gameObject, int.MaxValue));
    }

    void AlterLife()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.SendMessage("Heal", m_quantity);
        m_audioSource.Play();
        StartCoroutine(DestroyCallback());
    }

    void AlterMagic()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.SendMessage("AlterMana", m_quantity);
        m_audioSource.Play();
        StartCoroutine(DestroyCallback());
    }

    void AlterScore()
    {
        // todo: implement score items alter score
        m_audioSource.Play();
        StartCoroutine(DestroyCallback());
    }

    CollectionCallback m_callbackFunction;
}