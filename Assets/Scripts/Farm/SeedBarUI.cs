using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SeedBarUI : MonoBehaviour
{
    public static SeedBarUI Instance { get; private set; }

    [Header("UI Containers")]
    [Tooltip("Khung chứa thanh chọn hạt giống")]
    [SerializeField] private RectTransform m_Panel;

    [Tooltip("ScrollRect cuộn ngang danh sách hạt giống")]
    [SerializeField] private ScrollRect m_ScrollRect;

    [Tooltip("Nút đóng thanh hạt giống (nút X)")]
    [SerializeField] private Button m_CloseButton;

    [Header("Master Plants Database")]
    [Tooltip("Danh sách TẤT CẢ các loại cây có trong game (PlantData SO). Thanh bar sẽ chỉ hiển thị những loại có số lượng > 0 trong kho")]
    [SerializeField] private List<PlantData> m_AllPlants = new List<PlantData>();

    [Header("Dynamic Generation (Khuyên dùng)")]
    [Tooltip("Container chứa các item (đối tượng Content trong Viewport)")]
    [SerializeField] private Transform m_ItemsContainer;
    [Tooltip("Prefab của 1 thẻ hạt giống SeedItemUI")]
    [SerializeField] private SeedItemUI m_SeedItemPrefab;

    [Header("Static Items (Tùy chọn nếu tự tạo nút sẵn trong Scene)")]
    [SerializeField] private List<SeedItemUI> m_StaticItems = new List<SeedItemUI>();

    private PlantSlot _currentSlot;

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

        if (m_CloseButton != null)
        {
            m_CloseButton.onClick.AddListener(Close);
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Làm mới danh sách hiển thị chỉ dựa trên các hạt giống đang có trong kho (số lượng > 0)
    /// </summary>
    public void RefreshOwnedSeeds()
    {
        if (m_AllPlants == null || m_AllPlants.Count == 0)
        {
            Debug.LogWarning("[SeedBarUI] Danh sách m_AllPlants đang trống! Hãy kéo các file PlantData vào Inspector.");
            return;
        }

        if (m_SeedItemPrefab != null && m_ItemsContainer != null)
        {
            // Xóa các item cũ
            foreach (Transform child in m_ItemsContainer)
            {
                Destroy(child.gameObject);
            }

            int displayedCount = 0;

            // Duyệt qua tất cả các loại hạt giống trong game
            foreach (var plant in m_AllPlants)
            {
                if (plant == null) continue;

                // Lấy số lượng hạt giống người chơi hiện đang có trong kho
                int seedCount = PlayerDataManager.Instance.GetSeedCount(plant.plantId);

                // CHỈ HIỂN THỊ NẾU CÒN HẠT GIỐNG (> 0)
                if (seedCount > 0)
                {
                    SeedItemUI item = Instantiate(m_SeedItemPrefab, m_ItemsContainer);
                    item.SetData(plant, seedCount);
                    displayedCount++;
                }
            }

            if (displayedCount == 0)
            {
                Debug.Log("[SeedBarUI] Bạn đã hết toàn bộ hạt giống!");
            }
        }
        else if (m_StaticItems != null && m_StaticItems.Count > 0)
        {
            // Nếu dùng các nút tĩnh có sẵn trong Scene
            int itemIdx = 0;
            foreach (var plant in m_AllPlants)
            {
                if (plant == null) continue;
                int count = PlayerDataManager.Instance.GetSeedCount(plant.plantId);

                if (count > 0 && itemIdx < m_StaticItems.Count)
                {
                    m_StaticItems[itemIdx].gameObject.SetActive(true);
                    m_StaticItems[itemIdx].SetData(plant, count);
                    itemIdx++;
                }
            }

            // Ẩn nút thừa
            for (int i = itemIdx; i < m_StaticItems.Count; i++)
            {
                if (m_StaticItems[i] != null)
                {
                    m_StaticItems[i].gameObject.SetActive(false);
                }
            }
        }

        // Ép Unity tính toán lại chiều dài của Content theo đúng số hạt giống đang hiển thị
        if (m_ItemsContainer is RectTransform rt)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

    /// <summary>
    /// Mở thanh chọn hạt giống khi bấm vào 1 ô đất trống
    /// </summary>
    public void Open(PlantSlot slot)
    {
        _currentSlot = slot;
        gameObject.SetActive(true);

        // Nạp và lọc lại danh sách hạt giống theo số lượng hiện có trong kho
        RefreshOwnedSeeds();

        // Đưa thanh cuộn về vị trí đầu tiên bên trái mỗi khi mở
        if (m_ScrollRect != null)
        {
            m_ScrollRect.horizontalNormalizedPosition = 0f;
        }

        if (m_Panel != null)
        {
            m_Panel.DOKill();
            m_Panel.localScale = Vector3.one * 0.85f;
            m_Panel.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }
    }

    public void Close()
    {
        if (m_Panel != null)
        {
            m_Panel.DOKill();
            m_Panel.DOScale(Vector3.one * 0.8f, 0.15f).SetEase(Ease.InBack).OnComplete(() =>
            {
                gameObject.SetActive(false);
                _currentSlot = null;
            });
        }
        else
        {
            gameObject.SetActive(false);
            _currentSlot = null;
        }
    }

    /// <summary>
    /// Được gọi khi người chơi bấm chọn 1 loại hạt giống để trồng
    /// </summary>
    public void SelectPlant(PlantData plant)
    {
        if (plant == null || _currentSlot == null) return;

        // Trừ 1 hạt giống trong kho
        bool usedSuccess = PlayerDataManager.Instance.UseSeed(plant.plantId, 1);
        if (usedSuccess)
        {
            // Tiến hành gieo hạt vào ô đất
            _currentSlot.Plant(plant);
        }
        else
        {
            Debug.LogWarning($"[SeedBarUI] Không thể gieo vì bạn đã hết hạt giống {plant.plantName}!");
        }

        Close();
    }
}
