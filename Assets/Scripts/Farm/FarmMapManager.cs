using System;
using System.Collections.Generic;
using UnityEngine;

public class FarmMapManager : MonoBehaviour
{
    public static FarmMapManager Instance { get; private set; }

    [Header("Island Maps")]
    [Tooltip("Danh sách các đảo/map trồng cây")]
    [SerializeField] private List<FarmMap> m_Maps = new List<FarmMap>();

    [Tooltip("Chỉ số map đang hiển thị")]
    [SerializeField] private int m_CurrentMapIndex = 0;

    public event Action<FarmMap> OnMapChanged;

    public FarmMap CurrentMap => (m_Maps != null && m_Maps.Count > m_CurrentMapIndex) ? m_Maps[m_CurrentMapIndex] : null;
    public int CurrentMapIndex => m_CurrentMapIndex;
    public int TotalMaps => m_Maps != null ? m_Maps.Count : 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeMaps();
        SetupChangeMapButton();
    }

    public void SetupChangeMapButton()
    {
        var btnGo = GameObject.Find("ChangeMapButton");
        if (btnGo != null)
        {
            if (btnGo.GetComponent<ChangeMapButton>() == null)
            {
                btnGo.AddComponent<ChangeMapButton>();
                Debug.Log("[FarmMapManager] Đã tự động gắn ChangeMapButton vào GameObject ChangeMapButton!");
            }
        }
        else
        {
            Debug.LogWarning("[FarmMapManager] Không tìm thấy GameObject ChangeMapButton trong Scene!");
        }
    }

    public void InitializeMaps()
    {
        if (m_Maps == null || m_Maps.Count == 0)
        {
            AutoFindMaps();
        }

        m_Maps.RemoveAll(m => m == null);

        if (m_Maps.Count == 0) return;

        // Giới hạn index hợp lệ
        m_CurrentMapIndex = Mathf.Clamp(m_CurrentMapIndex, 0, m_Maps.Count - 1);

        for (int i = 0; i < m_Maps.Count; i++)
        {
            if (m_Maps[i] != null)
            {
                if (i == m_CurrentMapIndex)
                {
                    m_Maps[i].Show(animate: false);
                }
                else
                {
                    m_Maps[i].Hide(animate: false);
                }
            }
        }

        OnMapChanged?.Invoke(CurrentMap);
    }

    /// <summary>
    /// Chuyển sang map kế tiếp (vòng lặp)
    /// </summary>
    public void NextMap()
    {
        if (m_Maps == null || m_Maps.Count <= 1)
        {
            AutoFindMaps();
        }

        m_Maps.RemoveAll(m => m == null);

        if (m_Maps == null || m_Maps.Count <= 1)
        {
            Debug.LogWarning($"[FarmMapManager] Không thể đổi map vì số lượng map là {m_Maps?.Count ?? 0} (cần ít nhất 2 map)!");
            return;
        }

        // Ẩn map hiện tại
        if (CurrentMap != null)
        {
            CurrentMap.Hide(animate: false);
        }

        // Tăng index theo vòng lặp
        m_CurrentMapIndex = (m_CurrentMapIndex + 1) % m_Maps.Count;

        // Hiện map mới với animation
        if (CurrentMap != null)
        {
            CurrentMap.Show(animate: true);
            Debug.Log($"[FarmMapManager] Đã chuyển sang: {CurrentMap.MapName} (Index: {m_CurrentMapIndex})");
        }

        OnMapChanged?.Invoke(CurrentMap);
    }

    /// <summary>
    /// Chuyển đến map theo index chỉ định
    /// </summary>
    public void ChangeMap(int index)
    {
        if (m_Maps == null || index < 0 || index >= m_Maps.Count || index == m_CurrentMapIndex) return;

        if (CurrentMap != null)
        {
            CurrentMap.Hide(animate: false);
        }

        m_CurrentMapIndex = index;

        if (CurrentMap != null)
        {
            CurrentMap.Show(animate: true);
        }

        OnMapChanged?.Invoke(CurrentMap);
    }

    [ContextMenu("Auto Find Maps")]
    public void AutoFindMaps()
    {
        m_Maps.Clear();
        FarmMap[] found = GetComponentsInChildren<FarmMap>(true);
        m_Maps.AddRange(found);
        Debug.Log($"[FarmMapManager] Đã tìm thấy {m_Maps.Count} maps trong đối tượng này.");
    }
}

/// <summary>
/// Component gắn trực tiếp vào GameObject ChangeMapButton để đảm bảo luôn nhận click và gọi NextMap
/// </summary>
[RequireComponent(typeof(UnityEngine.UI.Button))]
public class ChangeMapButton : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
{
    private UnityEngine.UI.Button _button;
    private int _lastClickFrame = -1;

    private void Awake()
    {
        _button = GetComponent<UnityEngine.UI.Button>();
        if (_button != null)
        {
            _button.onClick.RemoveListener(TriggerChangeMap);
            _button.onClick.AddListener(TriggerChangeMap);
        }
    }

    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Debug.Log("[ChangeMapButton] Click nhận từ IPointerClickHandler!");
        TriggerChangeMap();
    }

    public void TriggerChangeMap()
    {
        if (Time.frameCount == _lastClickFrame) return;
        _lastClickFrame = Time.frameCount;

        Debug.Log("[ChangeMapButton] Bắt đầu chuyển map...");

        if (FarmMapManager.Instance != null)
        {
            FarmMapManager.Instance.NextMap();
        }
        else
        {
            var manager = FindAnyObjectByType<FarmMapManager>();
            if (manager != null)
            {
                manager.NextMap();
            }
            else
            {
                Debug.LogError("[ChangeMapButton] Không tìm thấy FarmMapManager trong Scene!");
            }
        }
    }
}
