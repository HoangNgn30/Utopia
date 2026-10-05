using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantSlot : MonoBehaviour
{
    [Header("Slot Information")]
    [Tooltip("Chỉ số của ô trồng cây (từ 0 đến 8)")]
    [SerializeField] private int m_SlotIndex = 0;

    [Header("UI Visuals")]
    [Tooltip("Icon trồng cây (hình chiếc khiên có mầm cây) hiển thị khi ô còn trống")]
    [SerializeField] private Image m_PlantIcon;

    [Tooltip("Hình ảnh cây sẽ hiển thị sau khi đã trồng")]
    [SerializeField] private Image m_PlantVisual;

    [Tooltip("Text hiển thị thời gian đếm ngược (Tùy chọn)")]
    [SerializeField] private TextMeshProUGUI m_TimerText;

    [Tooltip("Nút bấm tương tác trên ô đất")]
    [SerializeField] private Button m_SlotButton;

    public int SlotIndex => m_SlotIndex;
    public bool IsPlanted { get; private set; } = false;
    public bool IsReadyToHarvest { get; private set; } = false;
    public PlantData CurrentPlant => m_CurrentPlantData;

    private PlantData m_CurrentPlantData;
    private float m_GrowthTimer = 0f;
    private int m_CurrentStageIndex = -1;
    private double m_PlantTimestamp = 0;

    private double GetCurrentTimestamp()
    {
        return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
    }

    public void SetSlotIndex(int index)
    {
        m_SlotIndex = index;
    }

    private void Awake()
    {
        if (m_SlotButton == null)
        {
            m_SlotButton = GetComponent<Button>();
        }

        if (m_SlotButton != null)
        {
            m_SlotButton.onClick.AddListener(OnSlotClicked);
        }

        UpdateVisualState();
    }

    private void OnEnable()
    {
        if (IsPlanted && m_CurrentPlantData != null)
        {
            // Ngay khi map được bật lại, tính toán ngay thời gian thực tế đã trôi qua
            RefreshGrowthState(animateTransition: false);

            if (IsReadyToHarvest && m_PlantVisual != null)
            {
                m_PlantVisual.transform.DOKill();
                m_PlantVisual.transform.localScale = Vector3.one;
                m_PlantVisual.transform.DOScale(Vector3.one * 1.15f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
        }
    }

    private void OnDisable()
    {
        if (m_PlantVisual != null)
        {
            m_PlantVisual.transform.DOKill();
        }
    }

    private void Update()
    {
        if (!IsPlanted || IsReadyToHarvest || m_CurrentPlantData == null) return;

        RefreshGrowthState(animateTransition: true);
    }

    /// <summary>
    /// Tính toán trạng thái lớn của cây dựa trên thời gian thực tế (UTC timestamp)
    /// </summary>
    public void RefreshGrowthState(bool animateTransition = false)
    {
        if (!IsPlanted || m_CurrentPlantData == null) return;

        if (m_PlantTimestamp > 0)
        {
            double elapsed = GetCurrentTimestamp() - m_PlantTimestamp;
            m_GrowthTimer = Mathf.Max(0f, (float)elapsed);
        }

        // Khi đạt 100% thời gian -> Cây chín
        if (m_GrowthTimer >= m_CurrentPlantData.growTimeInSeconds)
        {
            if (!IsReadyToHarvest)
            {
                OnPlantRipe();
            }
            return;
        }

        float progress = Mathf.Clamp01(m_GrowthTimer / m_CurrentPlantData.growTimeInSeconds);

        // Xác định giai đoạn lớn hiện tại (0 -> 3 tương ứng 4 sprite lớn dần)
        int stageIndex = Mathf.Clamp(Mathf.FloorToInt(progress * m_CurrentPlantData.growthStages.Length), 0, m_CurrentPlantData.growthStages.Length - 1);

        if (stageIndex != m_CurrentStageIndex)
        {
            m_CurrentStageIndex = stageIndex;
            if (m_PlantVisual != null && m_CurrentPlantData.growthStages != null && stageIndex < m_CurrentPlantData.growthStages.Length && m_CurrentPlantData.growthStages[stageIndex] != null)
            {
                m_PlantVisual.sprite = m_CurrentPlantData.growthStages[stageIndex];

                if (animateTransition)
                {
                    // Hiệu ứng nảy nhẹ khi cây bước sang giai đoạn lớn mới
                    m_PlantVisual.transform.DOKill();
                    m_PlantVisual.transform.localScale = Vector3.one;
                    m_PlantVisual.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 5, 1f);
                }
            }
        }

        // Cập nhật text đếm ngược nếu có
        if (m_TimerText != null)
        {
            float remainingSeconds = Mathf.Max(0, m_CurrentPlantData.growTimeInSeconds - m_GrowthTimer);
            m_TimerText.text = FormatTime(remainingSeconds);
        }
    }

    private void OnPlantRipe()
    {
        IsReadyToHarvest = true;

        if (m_PlantVisual != null && m_CurrentPlantData.growthStages.Length > 0)
        {
            // Hiển thị sprite giai đoạn cuối cùng (chín vàng)
            m_PlantVisual.sprite = m_CurrentPlantData.growthStages[^1];

            // Hiệu ứng nhún nhảy liên tục báo hiệu có thể thu hoạch
            m_PlantVisual.transform.DOKill();
            m_PlantVisual.transform.DOScale(Vector3.one * 1.15f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        if (m_TimerText != null)
        {
            m_TimerText.text = "Thu hoạch!";
        }

        Debug.Log($"[PlantSlot] {m_CurrentPlantData.plantName} ở ô {m_SlotIndex} đã chín! Có thể thu hoạch.");
    }

    private void OnSlotClicked()
    {
        if (!IsPlanted)
        {
            // Ô còn trống -> Mở thanh chọn hạt giống
            if (m_PlantIcon != null)
            {
                m_PlantIcon.transform.DOKill();
                m_PlantIcon.transform.localScale = Vector3.one;
                m_PlantIcon.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 8, 1f);
            }

            if (SeedBarUI.Instance != null)
            {
                SeedBarUI.Instance.Open(this);
            }
            else
            {
                Debug.LogWarning("[PlantSlot] Chưa tìm thấy SeedBarUI trong Scene!");
            }
        }
        else if (IsReadyToHarvest)
        {
            // Cây đã chín -> Thu hoạch
            Harvest();
        }
        else
        {
            // Cây đang lớn -> Bấm vào để xem thời gian còn lại
            if (m_PlantVisual != null)
            {
                m_PlantVisual.transform.DOKill();
                m_PlantVisual.transform.localScale = Vector3.one;
                m_PlantVisual.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 1f);
            }

            float remaining = Mathf.Max(0, m_CurrentPlantData.growTimeInSeconds - m_GrowthTimer);
            Debug.Log($"[PlantSlot] {m_CurrentPlantData.plantName} ở ô {m_SlotIndex} đang lớn... Còn {Mathf.CeilToInt(remaining)}s!");
        }
    }

    /// <summary>
    /// Gieo hạt giống vào ô
    /// </summary>
    public void Plant(PlantData plantData)
    {
        if (plantData == null) return;

        m_CurrentPlantData = plantData;
        IsPlanted = true;
        IsReadyToHarvest = false;
        m_PlantTimestamp = GetCurrentTimestamp();
        m_GrowthTimer = 0f;
        m_CurrentStageIndex = 0;

        // Ẩn icon huy hiệu mầm cây
        if (m_PlantIcon != null)
        {
            m_PlantIcon.gameObject.SetActive(false);
        }

        // Hiển thị mầm cây giai đoạn 1
        if (m_PlantVisual != null)
        {
            m_PlantVisual.gameObject.SetActive(true);
            if (plantData.growthStages != null && plantData.growthStages.Length > 0)
            {
                m_PlantVisual.sprite = plantData.growthStages[0];
            }

            // Hiệu ứng mầm cây nảy nở
            m_PlantVisual.transform.DOKill();
            m_PlantVisual.transform.localScale = Vector3.zero;
            m_PlantVisual.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }

        if (m_TimerText != null)
        {
            m_TimerText.gameObject.SetActive(true);
            m_TimerText.text = FormatTime(plantData.growTimeInSeconds);
        }

        Debug.Log($"[PlantSlot] Đã gieo {plantData.plantName} vào ô {m_SlotIndex}. Thời gian lớn: {plantData.growTimeInSeconds}s.");
    }

    /// <summary>
    /// Thu hoạch nông sản
    /// </summary>
    public void Harvest()
    {
        if (!IsPlanted || !IsReadyToHarvest || m_CurrentPlantData == null) return;

        int yieldCount = m_CurrentPlantData.harvestYield;

        // Lưu sản lượng vào kho đồ
        PlayerDataManager.Instance.AddCrop(m_CurrentPlantData.plantId, yieldCount);

        // Hiệu ứng thu hoạch cây bay lên và biến mất
        if (m_PlantVisual != null)
        {
            m_PlantVisual.transform.DOKill();
            m_PlantVisual.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
            {
                ClearPlant();
            });
        }
        else
        {
            ClearPlant();
        }

        Debug.Log($"[PlantSlot] Thu hoạch thành công {yieldCount}x {m_CurrentPlantData.plantName}!");
    }

    public void ClearPlant()
    {
        IsPlanted = false;
        IsReadyToHarvest = false;
        m_CurrentPlantData = null;
        m_PlantTimestamp = 0;
        m_GrowthTimer = 0f;
        m_CurrentStageIndex = -1;

        if (m_PlantVisual != null)
        {
            m_PlantVisual.transform.DOKill();
            m_PlantVisual.transform.localScale = Vector3.one;
            m_PlantVisual.gameObject.SetActive(false);
        }

        if (m_PlantIcon != null)
        {
            m_PlantIcon.gameObject.SetActive(true);
            m_PlantIcon.transform.DOKill();
            m_PlantIcon.transform.localScale = Vector3.zero;
            m_PlantIcon.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }

        if (m_TimerText != null)
        {
            m_TimerText.text = "";
            m_TimerText.gameObject.SetActive(false);
        }
    }

    private void UpdateVisualState()
    {
        if (m_PlantIcon != null) m_PlantIcon.gameObject.SetActive(!IsPlanted);
        if (m_PlantVisual != null) m_PlantVisual.gameObject.SetActive(IsPlanted);
        if (m_TimerText != null) m_TimerText.gameObject.SetActive(IsPlanted);
    }

    private string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return mins > 0 ? $"{mins:D2}:{secs:D2}" : $"{secs}s";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = IsReadyToHarvest ? Color.yellow : (IsPlanted ? Color.cyan : Color.green);
        Gizmos.DrawWireSphere(transform.position, 25f);
    }
#endif
}
