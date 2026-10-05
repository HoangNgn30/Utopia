using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupShop : BasePopup
{
    [Header("Special Packs")]
    [SerializeField] private Button m_BuyADSButton;
    [SerializeField] private Button m_BuyKingdomPassButton;

    [Header("Coin Packs (Single Currency)")]
    [SerializeField] private Button m_Buy100CoinButton;
    [SerializeField] private Button m_Buy2100CoinButton;
    [SerializeField] private Button m_Buy12000CoinButton;
    [SerializeField] private Button m_BuyMonthlySubButton;

    [Header("Free Coin")]
    [SerializeField] private Button m_FreeCoinButton;
    [SerializeField] private TextMeshProUGUI m_QuantityCoinFree;
    [SerializeField] private TextMeshProUGUI m_FreeRemaining;

    #region Button Actions

    public void BuyADS()
    {
        Debug.Log("[PopupShop] Mua gói Bỏ qua quảng cáo");
    }

    public void BuyKingdomPass()
    {
        Debug.Log("[PopupShop] Mua Thẻ Nông Trại VIP");
    }

    public void Buy100Coin()
    {
        PlayerDataManager.Instance.AddCoin(100);
    }

    public void Buy2100Coin()
    {
        PlayerDataManager.Instance.AddCoin(2100);
    }

    public void Buy12000Coin()
    {
        PlayerDataManager.Instance.AddCoin(12000);
    }

    public void BuyMonthlySub()
    {
        Debug.Log("[PopupShop] Đăng ký gói tháng");
    }

    public void CollectFreeCoin()
    {
        PlayerDataManager.Instance.AddCoin(50);
    }

    #endregion

    public void SetData(int quantityCoinFree, int freeRemaining)
    {
        if (m_QuantityCoinFree != null) m_QuantityCoinFree.text = $"{quantityCoinFree}";
        if (m_FreeRemaining != null) m_FreeRemaining.text = $"Daily limit {freeRemaining}/3";
    }

    private void Start()
    {
        if (m_BuyADSButton != null) m_BuyADSButton.onClick.AddListener(BuyADS);
        if (m_BuyKingdomPassButton != null) m_BuyKingdomPassButton.onClick.AddListener(BuyKingdomPass);
        if (m_Buy100CoinButton != null) m_Buy100CoinButton.onClick.AddListener(Buy100Coin);
        if (m_Buy2100CoinButton != null) m_Buy2100CoinButton.onClick.AddListener(Buy2100Coin);
        if (m_Buy12000CoinButton != null) m_Buy12000CoinButton.onClick.AddListener(Buy12000Coin);
        if (m_BuyMonthlySubButton != null) m_BuyMonthlySubButton.onClick.AddListener(BuyMonthlySub);
        if (m_FreeCoinButton != null) m_FreeCoinButton.onClick.AddListener(CollectFreeCoin);
    }
}
