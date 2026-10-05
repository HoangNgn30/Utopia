using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupProfile : BasePopup
{
    [Header("Profile Info")]
    [SerializeField] private TextMeshProUGUI m_PlayerNameText;
    [SerializeField] private TextMeshProUGUI m_LevelText;
    [SerializeField] private TextMeshProUGUI m_ExpText;
    [SerializeField] private TextMeshProUGUI m_CoinsText;
    [SerializeField] private TMP_InputField m_PlayerNameInput;
    [SerializeField] private Button m_BackButton;
    [SerializeField] private Button m_SaveNameButton;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (m_BackButton != null) m_BackButton.onClick.AddListener(OnClickBackButton);
        if (m_SaveNameButton != null) m_SaveNameButton.onClick.AddListener(OnClickSaveName);

        UpdateProfileUI();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateProfileUI();
    }

    public void UpdateProfileUI()
    {
        var data = PlayerDataManager.Instance;
        if (data == null) return;

        if (m_PlayerNameText != null) m_PlayerNameText.text = data.PlayerName;
        if (m_LevelText != null) m_LevelText.text = $"Cấp {data.Level}";
        if (m_ExpText != null) m_ExpText.text = $"{data.Exp} / {data.ExpToNextLevel} EXP";
        if (m_CoinsText != null) m_CoinsText.text = data.Coins.ToString("N0");

        if (m_PlayerNameInput != null) m_PlayerNameInput.text = data.PlayerName;
    }

    public void OnClickSaveName()
    {
        if (m_PlayerNameInput != null && !string.IsNullOrWhiteSpace(m_PlayerNameInput.text))
        {
            PlayerDataManager.Instance.SetPlayerName(m_PlayerNameInput.text.Trim());
            UpdateProfileUI();
        }
    }

    public void OnClickBackButton()
    {
        Close();
    }
}
