using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TabItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image m_BgImage;
    [SerializeField] private RectTransform m_ContentContainer;
    [SerializeField] private TextMeshProUGUI m_Label;
    [SerializeField] private LayoutElement m_LayoutElement; 

    [Header("Visual Settings")]
    [SerializeField] private Sprite m_ActiveBackground;
    [SerializeField] private Sprite m_NormalBackground;
    
    [Header("Size Settings")]
    [SerializeField] private float m_ButtonActiveWidth = 250f; 
    [SerializeField] private float m_ButtonNormalWidth = 150f; 
    [SerializeField] private float m_Duration = 0.25f;

    private Button m_Button;
    private float m_TargetScaleActive = 1.5f;
    private float m_TargetScaleInactive = 1f;


    #region Button
    public void RegisterClick(System.Action<TabItem> onClickAction)
    {
        m_Button.onClick.AddListener(() => onClickAction?.Invoke(this));
    }

    public void SetData(bool isActive, bool immediate = false)
    {
        m_BgImage.sprite = isActive ? m_ActiveBackground : m_NormalBackground;

        float targetWidth = isActive ? m_ButtonActiveWidth : m_ButtonNormalWidth;
        float targetScale = isActive ? m_TargetScaleActive : m_TargetScaleInactive;

        if (immediate) 
        {
            // Gán thẳng giá trị nếu là khởi tạo lần đầu
            m_LayoutElement.preferredWidth = targetWidth;
            m_ContentContainer.localScale = Vector3.one * targetScale;
        }
        else 
        {
            DOTween.To(() => m_LayoutElement.preferredWidth, x => m_LayoutElement.preferredWidth = x, targetWidth, m_Duration);
            m_ContentContainer.DOScale(targetScale, m_Duration).SetEase(Ease.OutBack);
        }

        // if (isActive) transform.SetAsLastSibling();
    }

    #endregion

    private void Awake()
    {
        m_Button = GetComponent<Button>();
        m_LayoutElement = GetComponent<LayoutElement>();
    }
}