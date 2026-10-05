using System.Collections.Generic;
using UnityEngine;

public class Footer : MonoBehaviour
{
    [SerializeField] private List<TabItem> m_Tabs;
    public System.Action<int> OnTabChanged;


    #region Button
    public void DefaultTab()
    {

        UpdateTabsUI(m_Tabs[2]); // Nút Play
        OnTabChanged?.Invoke(2);

    }

    public void SelectTab(int index)
    {
        UpdateTabsUI(m_Tabs[index]);
        OnTabChanged?.Invoke(index);
    }

    private void UpdateTabsUI(TabItem selectedTab)
    {
        foreach (var tab in m_Tabs)
        {
            tab.SetData(tab == selectedTab);
        }
    }

    #endregion


    void Start()
    {
        for (int i = 0; i < m_Tabs.Count; i++)
        {
            int index = i;
            m_Tabs[i].RegisterClick((clickedTab) =>
            {
                UpdateTabsUI(clickedTab);
                OnTabChanged?.Invoke(index);
            });
        }


        Invoke(nameof(DefaultTab), 0.05f);
    }
}