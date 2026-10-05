using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager INSTANCE;

    [SerializeField] private Transform m_PopupParent;

    [SerializeField] private int m_SortingOrder = 500;

    public Transform PopupParent => m_PopupParent;

    public T ShowPopup<T>(T prefab) where T : Object
    {
        UpdateCanvasSorting();
        return Instantiate(prefab, m_PopupParent);
    }

    public void ClearAll()
    {
        if (m_PopupParent == null) return;
        foreach (Transform child in m_PopupParent)
            Destroy(child.gameObject);
    }

    private void Awake()
    {
        if (INSTANCE == null) INSTANCE = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        UpdateCanvasSorting();
    }

    private void Start()
    {
        UpdateCanvasSorting();
    }

    public void UpdateCanvasSorting()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null && m_PopupParent != null) canvas = m_PopupParent.GetComponent<Canvas>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = m_SortingOrder;
        }
    }
}
