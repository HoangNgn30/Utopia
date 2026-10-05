using System;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    private static PlayerDataManager _instance;
    public static PlayerDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PlayerDataManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerDataManager");
                    _instance = go.AddComponent<PlayerDataManager>();
                }
            }
            return _instance;
        }
    }

    public static event Action<long> OnCoinChanged;
    public static event Action<int> OnLevelChanged;
    public static event Action<long, long> OnExpChanged;

    private const string PREF_COINS = "Utopia_Coins";
    private const string PREF_LEVEL = "Utopia_Level";
    private const string PREF_EXP = "Utopia_Exp";
    private const string PREF_PLAYER_NAME = "Utopia_PlayerName";

    public string PlayerName { get; private set; }
    public int Level { get; private set; }
    public long Exp { get; private set; }
    public long Coins { get; private set; }

    public long ExpToNextLevel => Level * 100L;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    public void LoadData()
    {
        PlayerName = PlayerPrefs.GetString(PREF_PLAYER_NAME, "Nông Dân Utopia");
        Level = PlayerPrefs.GetInt(PREF_LEVEL, 1);
        Exp = Convert.ToInt64(PlayerPrefs.GetString(PREF_EXP, "0"));
        Coins = Convert.ToInt64(PlayerPrefs.GetString(PREF_COINS, "1000"));

        // Tự động cấp 9 hạt lúa (ID: 0) và 3 hạt cà chua (ID: 1) ban đầu nếu chưa có
        if (GetSeedCount(0) <= 0)
        {
            PlayerPrefs.SetInt(FormatSeedKey(0), 9);
        }

        if (GetSeedCount(1) <= 0)
        {
            PlayerPrefs.SetInt(FormatSeedKey(1), 3);
        }
        PlayerPrefs.Save();
    }

    public void SaveData()
    {
        PlayerPrefs.SetString(PREF_PLAYER_NAME, PlayerName);
        PlayerPrefs.SetInt(PREF_LEVEL, Level);
        PlayerPrefs.SetString(PREF_EXP, Exp.ToString());
        PlayerPrefs.SetString(PREF_COINS, Coins.ToString());
        PlayerPrefs.Save();
    }

    public void SetPlayerName(string newName)
    {
        PlayerName = newName;
        SaveData();
    }

    public void AddCoin(long amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        SaveData();
        OnCoinChanged?.Invoke(Coins);
    }

    public bool SpendCoin(long amount)
    {
        if (amount <= 0 || Coins < amount) return false;
        Coins -= amount;
        SaveData();
        OnCoinChanged?.Invoke(Coins);
        return true;
    }

    public void AddExp(long amount)
    {
        if (amount <= 0) return;
        Exp += amount;

        bool leveledUp = false;
        while (Exp >= ExpToNextLevel)
        {
            Exp -= ExpToNextLevel;
            Level++;
            leveledUp = true;
        }

        SaveData();
        if (leveledUp) OnLevelChanged?.Invoke(Level);
        OnExpChanged?.Invoke(Exp, ExpToNextLevel);
    }

    #region Crop Inventory
    public int GetCropCount(int plantId)
    {
        return PlayerPrefs.GetInt("Crop_" + plantId, 0);
    }

    public void AddCrop(int plantId, int amount)
    {
        if (amount <= 0) return;
        int current = GetCropCount(plantId);
        PlayerPrefs.SetInt("Crop_" + plantId, current + amount);
        PlayerPrefs.Save();
        Debug.Log($"[PlayerDataManager] Đã thêm {amount}x nông sản (ID: {plantId}) vào kho. Tổng hiện có: {current + amount}");
    }

    public bool SellCrop(int plantId, int amount, int unitPrice)
    {
        int current = GetCropCount(plantId);
        if (amount <= 0 || current < amount) return false;

        PlayerPrefs.SetInt("Crop_" + plantId, current - amount);
        PlayerPrefs.Save();

        long earnedCoins = (long)amount * unitPrice;
        AddCoin(earnedCoins);
        Debug.Log($"[PlayerDataManager] Đã bán {amount}x nông sản (ID: {plantId}) thu về {earnedCoins} coin.");
        return true;
    }
    #endregion

    #region Seed Inventory
    private string FormatSeedKey(int plantId)
    {
        return $"Seed_{plantId}";
    }

    public int GetSeedCount(int plantId)
    {
        return PlayerPrefs.GetInt(FormatSeedKey(plantId), 0);
    }

    public void AddSeed(int plantId, int amount)
    {
        if (amount <= 0) return;
        int current = GetSeedCount(plantId);
        PlayerPrefs.SetInt(FormatSeedKey(plantId), current + amount);
        PlayerPrefs.Save();
        Debug.Log($"[PlayerDataManager] Đã thêm {amount} hạt giống (ID: {plantId}). Tổng: {current + amount}");
    }

    public bool UseSeed(int plantId, int amount = 1)
    {
        int current = GetSeedCount(plantId);
        if (amount <= 0 || current < amount) return false;

        PlayerPrefs.SetInt(FormatSeedKey(plantId), current - amount);
        PlayerPrefs.Save();
        Debug.Log($"[PlayerDataManager] Đã dùng {amount} hạt giống (ID: {plantId}). Còn lại: {current - amount}");
        return true;
    }

    [ContextMenu("Grant Starter Seeds (9 Lua [0] + 3 Tomato [1])")]
    public void GrantStarterSeeds()
    {
        PlayerPrefs.SetInt(FormatSeedKey(0), 9);
        PlayerPrefs.SetInt(FormatSeedKey(1), 3);
        PlayerPrefs.Save();
        Debug.Log("[PlayerDataManager] Đã cấp 9 hạt Lúa (ID 0) và 3 hạt Cà Chua (ID 1)!");
    }
    #endregion
}
