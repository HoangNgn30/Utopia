using UnityEngine;

[CreateAssetMenu(fileName = "Plant_New", menuName = "Utopia/Plant Data", order = 1)]
public class PlantData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Mã định danh duy nhất dạng số nguyên (bắt đầu từ 0: 0 = Lúa, 1 = Cà Chua, ...)")]
    public int plantId = 0;

    [Tooltip("Tên hiển thị (vd: Lúa, Cà Chua)")]
    public string plantName = "Lúa";

    [Header("Sprites (SpriteSheet Cắt 6 phần tử)")]
    [Tooltip("Phần tử [0]: Túi hạt giống hiển thị trong Shop/SeedBar")]
    public Sprite seedBagIcon;

    [Tooltip("Phần tử [1] -> [4]: 4 giai đoạn cây lớn dần")]
    public Sprite[] growthStages = new Sprite[4];

    [Tooltip("Phần tử [5]: Icon vật phẩm nông sản dùng trong túi đồ")]
    public Sprite harvestItemIcon;

    [Header("Economy & Time")]
    [Tooltip("Thời gian cây chín hoàn toàn (tính bằng giây: 1 phút = 60s, 3 phút = 180s)")]
    public float growTimeInSeconds = 60f;

    [Tooltip("Giá mua hạt giống trong Shop (coin)")]
    public int seedPrice = 5;

    [Tooltip("Giá bán nông sản ra thị trường (coin)")]
    public int sellPrice = 3;

    [Tooltip("Sản lượng thu hoạch được mỗi lần")]
    public int harvestYield = 3;

    /// <summary>
    /// Lấy sprite hiển thị theo % tiến độ lớn (từ 0.0f đến 1.0f)
    /// </summary>
    public Sprite GetSpriteForProgress(float progress)
    {
        if (growthStages == null || growthStages.Length == 0) return null;

        int stageCount = growthStages.Length;
        int index = Mathf.Clamp(Mathf.FloorToInt(progress * stageCount), 0, stageCount - 1);
        return growthStages[index];
    }

    public int GetPlantId()
    {
        return plantId;
    }
}
