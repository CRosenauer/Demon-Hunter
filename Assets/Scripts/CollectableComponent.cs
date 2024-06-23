using System.Collections;
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
        SerializedProperty m_scoreScriptableObject;

        void OnEnable()
        {
            m_audioSource = serializedObject.FindProperty("m_audioSource");
            m_callback = serializedObject.FindProperty("m_callback");
            m_quantity = serializedObject.FindProperty("m_quantity");
            m_item = serializedObject.FindProperty("m_item");
            m_scoreScriptableObject = serializedObject.FindProperty("m_scoreScriptableObject");
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

            EditorGUILayout.PropertyField(m_audioSource);

            if (collectableComponent.m_callback == CollectableFunction.AlterScore)
            {
                EditorGUILayout.PropertyField(m_scoreScriptableObject);
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
    [SerializeField] ScoreChangedEvent m_scoreScriptableObject;

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

        m_audioSource.Play();
        player.BroadcastMessage("SetSecondaryWeapon", m_item);
        StartCoroutine(DestroyCallback());
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
        m_scoreScriptableObject.Raise(m_quantity);
        m_audioSource.Play();
        StartCoroutine(DestroyCallback());
    }

    CollectionCallback m_callbackFunction;
}