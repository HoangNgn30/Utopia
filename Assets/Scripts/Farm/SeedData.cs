using System;
using UnityEngine;

[Serializable]
public class SeedData
{
    public string seedId = "sunflower";
    public string seedName = "Hướng Dương";
    [Tooltip("Icon hiển thị trên thanh chọn hạt giống")]
    public Sprite seedIcon;
    [Tooltip("Hình ảnh cây sẽ hiển thị ở ô đất sau khi trồng")]
    public Sprite plantSprite;
    public int price = 50;
}
