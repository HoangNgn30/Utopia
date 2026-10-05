using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FarmMap : MonoBehaviour
{
    [Header("Map Info")]
    [SerializeField] private int m_MapId = 0;
    [SerializeField] private string m_MapName = "Đảo Mây 1";

    [Header("9 Plant Slots")]
    [Tooltip("Danh sách đúng 9 ô trồng cây do bạn đặt thủ công")]
    [SerializeField] private List<PlantSlot> m_PlantSlots = new List<PlantSlot>();

    public int MapId => m_MapId;
    public string MapName => m_MapName;
    public IReadOnlyList<PlantSlot> PlantSlots => m_PlantSlots;

    public void Show(bool animate = true)
    {
        gameObject.SetActive(true);

        if (animate)
        {
            transform.DOKill();
            transform.localScale = Vector3.one * 0.85f;
            transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    public void Hide(bool animate = false)
    {
        if (animate)
        {
            transform.DOKill();
            transform.DOScale(Vector3.one * 0.8f, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            transform.DOKill();
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Hàm tiện ích: Tự động tìm tất cả 9 PlantSlot con trong Inspector
    /// Click chuột phải vào component FarmMap chọn 'Auto Find 9 Slots'
    /// </summary>
    [ContextMenu("Auto Find 9 Slots")]
    public void AutoFindSlots()
    {
        m_PlantSlots.Clear();
        PlantSlot[] foundSlots = GetComponentsInChildren<PlantSlot>(true);

        for (int i = 0; i < foundSlots.Length; i++)
        {
            foundSlots[i].SetSlotIndex(i);
            m_PlantSlots.Add(foundSlots[i]);
        }

        Debug.Log($"[FarmMap] Đã tự động gán {m_PlantSlots.Count} slots cho {m_MapName}!");
        if (m_PlantSlots.Count != 9)
        {
            Debug.LogWarning($"[FarmMap] Chú ý: {m_MapName} hiện có {m_PlantSlots.Count} slots (yêu cầu chuẩn là 9).");
        }
    }
}
