using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeedItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [Tooltip("Icon túi hạt giống (Lua_0, Tomato_0)")]
    [SerializeField] private Image m_Icon;

    [Tooltip("Text hiển thị số lượng hạt giống đang có ở góc dưới bên trái icon")]
    [SerializeField] private TextMeshProUGUI m_QuantityText;

    [Header("Hover Tooltip (Chỉ hiện khi hover vào túi hạt giống)")]
    [Tooltip("Khung chứa thông tin tooltip (tùy chọn, để bật/tắt nguyên cụm)")]
    [SerializeField] private GameObject m_HoverInfoContainer;

    [Tooltip("Tên cây")]
    [SerializeField] private TextMeshProUGUI m_NameText;

    [Tooltip("Thời gian chín")]
    [SerializeField] private TextMeshProUGUI m_TimeText;

    [SerializeField] private Button m_Button;

    private PlantData _plantData;
    private int _quantity = 0;

    public PlantData PlantData => _plantData;
    public int Quantity => _quantity;

    private void Awake()
    {
        if (m_Button == null) m_Button = GetComponent<Button>();
        if (m_Button != null)
        {
            m_Button.onClick.AddListener(OnClick);
        }

        SetHoverInfoVisible(false);
    }

    private void OnDisable()
    {
        SetHoverInfoVisible(false);
    }

    public void SetData(PlantData plant, int quantity)
    {
        _plantData = plant;
        _quantity = quantity;
        if (plant == null) return;

        if (m_Icon != null && plant.seedBagIcon != null)
        {
            m_Icon.sprite = plant.seedBagIcon;
        }

        if (m_QuantityText != null)
        {
            m_QuantityText.text = $"x{quantity}";
        }

        if (m_NameText != null)
        {
            m_NameText.text = plant.plantName;
        }

        if (m_TimeText != null)
        {
            int mins = Mathf.FloorToInt(plant.growTimeInSeconds / 60);
            int secs = Mathf.FloorToInt(plant.growTimeInSeconds % 60);
            m_TimeText.text = mins > 0 ? $"{mins}m" : $"{secs}s";
        }

        // Mặc định ẩn thông tin khi khởi tạo
        SetHoverInfoVisible(false);
    }

    /// <summary>
    /// Khi chuột rê vào túi hạt giống (Hover)
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHoverInfoVisible(true);

        // Hiệu ứng phóng to nhẹ túi hạt giống khi hover
        if (m_Icon != null)
        {
            m_Icon.transform.DOKill();
            m_Icon.transform.DOScale(Vector3.one * 1.12f, 0.15f).SetEase(Ease.OutBack);
        }
    }

    /// <summary>
    /// Khi chuột rời khỏi túi hạt giống
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        SetHoverInfoVisible(false);

        if (m_Icon != null)
        {
            m_Icon.transform.DOKill();
            m_Icon.transform.DOScale(Vector3.one, 0.15f);
        }
    }

    private void SetHoverInfoVisible(bool visible)
    {
        if (m_HoverInfoContainer != null)
        {
            m_HoverInfoContainer.SetActive(visible);
        }
        else
        {
            if (m_NameText != null) m_NameText.gameObject.SetActive(visible);
            if (m_TimeText != null) m_TimeText.gameObject.SetActive(visible);
        }
    }

    private void OnClick()
    {
        if (_plantData == null || SeedBarUI.Instance == null) return;

        SetHoverInfoVisible(false);

        // Gieo hạt giống vào slot đang chọn
        SeedBarUI.Instance.SelectPlant(_plantData);
    }
}
