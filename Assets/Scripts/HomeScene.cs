using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeScene : MonoBehaviour
{
    [Header("Header - Profile & Level")]

    [SerializeField] private TextMeshProUGUI m_userName;
    [SerializeField] private TextMeshProUGUI m_level;
    [SerializeField] private TextMeshProUGUI m_exp;

    [Header("Header - Coins")]
    [SerializeField] private TextMeshProUGUI m_coin;

    [Header("Middle Buttons")]
    [SerializeField] private Button m_QuestsButton;
    [SerializeField] private Button m_ChangeMapButton;

    [Header("Farm & Map")]
    [SerializeField] private FarmMapManager m_FarmMapManager;

    [Header("Navigation")]
    [SerializeField] private Footer m_Footer;

    [Header("Tab Views")]
    [SerializeField] private Button m_BagButton;
    [SerializeField] private Button m_RanchButton;
    [SerializeField] private Button m_FarmButton;
    [SerializeField] private Button m_ShopButton;
    [SerializeField] private Button m_LoverButton;

    [Header("Popups & Containers")]
    [SerializeField] private Transform miniPopup;

    private long _currentCoins;

    public static void AddCoin(long amount) => PlayerDataManager.Instance.AddCoin(amount);
    public static void AddExp(long amount) => PlayerDataManager.Instance.AddExp(amount);



    private void OnEnable()
    {
        PlayerDataManager.OnCoinChanged += HandleCoinChanged;
        PlayerDataManager.OnLevelChanged += OnLevelChanged;
        PlayerDataManager.OnExpChanged += OnExpChanged;
    }

    private void OnDisable()
    {
        PlayerDataManager.OnCoinChanged -= HandleCoinChanged;
        PlayerDataManager.OnLevelChanged -= OnLevelChanged;
        PlayerDataManager.OnExpChanged -= OnExpChanged;

        if (m_Footer != null)
        {
            m_Footer.OnTabChanged -= HandleTabChanged;
        }
    }

    private void OnLevelChanged(int level) => UpdateProfileUI();
    private void OnExpChanged(long exp, long expToNext) => UpdateProfileUI();



    private void HandleCoinChanged(long newCoin)
    {
        _currentCoins = newCoin;
        UpdateCoinUI(animate: true);
    }

    private void UpdateCoinUI(bool animate = false)
    {
        if (m_coin == null) return;

        m_coin.text = _currentCoins.ToString("N0");
        if (animate)
        {
            m_coin.transform.DOKill();
            m_coin.transform.localScale = Vector3.one;
            m_coin.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 10, 1f);
        }
    }

    public void UpdateProfileUI()
    {
        var data = PlayerDataManager.Instance;
        if (data == null) return;

        m_userName.text = data.PlayerName;
        m_level.text = data.Level.ToString();

        float percent = data.ExpToNextLevel > 0 ? ((float)data.Exp / data.ExpToNextLevel) * 100f : 0f;
        m_exp.text = $"{Mathf.FloorToInt(percent)}%";
        
    }

    #region Button 

    public void OnClickBagButton()
    {
        //
    }

    public void OnClickRanchButton()
    {
        //
    }

    public void OnClickFarmButton()
    {
        //
    }

    private PopupShop _ShopPS;
    public void OnClickShopButton()
    {
        int FreeRemaining = 1;
        int QuantityCoinFree = 14;

        if (_ShopPS == null)
        {
            string popupPath = "Prefabs/Popup_Shop";

            GameObject loadGO = BundleHandler.LoadGameObject(popupPath);
            PopupShop aPS = loadGO.GetComponent<PopupShop>();
            if (aPS == null)
            {
                Debug.LogError("Sai duong dan");
                return;
            }
            _ShopPS = Instantiate(aPS, miniPopup);
            _ShopPS.SetData(QuantityCoinFree, FreeRemaining);
        }
        else
        {
            _ShopPS.gameObject.SetActive(true);
        }


    }

    public void OnClickLoversButton()
    {
        // string popupPath = "Prefabs/Popup_DailyChallenge";

        // GameObject loadGO = BundleHandler.LoadGameObject(popupPath);
        // PopupDailyChallenge aPDC = loadGO.GetComponent<PopupDailyChallenge>();
        // PopupDailyChallenge popup = PopupManager.INSTANCE.ShowPopup(aPDC);
        

        // popup.OnClose = () =>
        // {
        //     m_Footer.DefaultTab();
        // };
    }

    public void OnClickQuestsButton()
    {
        //
    }

    public void OnClickChangeMapButton()
    {
        if (m_FarmMapManager == null)
        {
            m_FarmMapManager = FarmMapManager.Instance != null ? FarmMapManager.Instance : FindAnyObjectByType<FarmMapManager>();
        }

        if (m_FarmMapManager != null)
        {
            Debug.Log("[HomeScene] Bấm nút ChangeMapButton -> Chuyển map...");
            m_FarmMapManager.NextMap();
        }
        else
        {
            Debug.LogWarning("[HomeScene] Không tìm thấy FarmMapManager trong scene!");
        }
    }

    #endregion


    private void HandleTabChanged(int index)
    {
        if (index != 3 && _ShopPS != null)
        {
            _ShopPS.gameObject.SetActive(false);
        }
    }

    #region Life 
    private void Start()
    {
        UpdateCoinUI(animate: false);
        UpdateProfileUI();

        if (m_QuestsButton != null)
        {
            m_QuestsButton.onClick.AddListener(OnClickQuestsButton);
            if (UIManager.INSTANCE != null && UIManager.INSTANCE.m_ScalingSUIC != null)
            {
                UIManager.INSTANCE.m_ScalingSUIC.AddASelectable(m_QuestsButton);
            }
        }

        if (m_ChangeMapButton == null)
        {
            GameObject btnGo = GameObject.Find("ChangeMapButton");
            if (btnGo != null)
            {
                m_ChangeMapButton = btnGo.GetComponent<Button>();
            }
        }

        if (m_ChangeMapButton != null)
        {
            m_ChangeMapButton.onClick.RemoveListener(OnClickChangeMapButton);
            m_ChangeMapButton.onClick.AddListener(OnClickChangeMapButton);
            Debug.Log("[HomeScene] Đã liên kết sự kiện OnClick cho ChangeMapButton thành công!");
        }
        else
        {
            Debug.LogWarning("[HomeScene] Không tìm thấy ChangeMapButton trong scene!");
        }

        if (m_BagButton != null)
        {
            m_BagButton.onClick.AddListener(OnClickBagButton);
            UIManager.INSTANCE.m_SoundClickOnlySUIC.AddASelectable(m_BagButton);
        }

        if (m_RanchButton != null)
        {
            m_RanchButton.onClick.AddListener(OnClickRanchButton);
            UIManager.INSTANCE.m_SoundClickOnlySUIC.AddASelectable(m_RanchButton);
        }

        if (m_FarmButton != null)
        {
            m_FarmButton.onClick.AddListener(OnClickFarmButton);
            UIManager.INSTANCE.m_SoundClickOnlySUIC.AddASelectable(m_FarmButton);
        }

        if (m_ShopButton != null)
        {
            m_ShopButton.onClick.AddListener(OnClickShopButton);
            UIManager.INSTANCE.m_SoundClickOnlySUIC.AddASelectable(m_ShopButton);
        }

        if (m_LoverButton != null)
        {
            m_LoverButton.onClick.AddListener(OnClickLoversButton);
            UIManager.INSTANCE.m_SoundClickOnlySUIC.AddASelectable(m_LoverButton);
        }

        if (m_Footer != null)
        {
            m_Footer.OnTabChanged += HandleTabChanged;
        }
    }

    private void Awake()
    {
        _currentCoins = PlayerDataManager.Instance.Coins;

        if (m_FarmMapManager == null)
        {
            m_FarmMapManager = FarmMapManager.Instance != null ? FarmMapManager.Instance : FindAnyObjectByType<FarmMapManager>();
        }

        if (m_ChangeMapButton == null)
        {
            GameObject btnGo = GameObject.Find("ChangeMapButton");
            if (btnGo != null)
            {
                m_ChangeMapButton = btnGo.GetComponent<Button>();
            }
        }
    }

    #endregion
}

/// <summary>
/// Alias class để đảm bảo Unity Inspector không bị lỗi Missing Script nếu scene tham chiếu HomeController
/// </summary>
public class HomeController : HomeScene { }
