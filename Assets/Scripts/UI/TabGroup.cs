using UnityEngine;
using System.Collections.Generic;

public class TabGroup : MonoBehaviour
{
    [Header("Tabs")]
    public List<TabButton> tabButtons = new List<TabButton>();

    [Header("Tab Colors")]
    public Color tabIdle;
    public Color tabHover;
    public Color tabActive;

    [Header("Objects to Swap")]
    [Tooltip("Assign all panels or objects to swap when tabs are selected.")]
    public List<GameObject> objectsToSwap = new List<GameObject>();

    public TabButton selectedTab;

    public void Subscribe(TabButton button)
    {
        if (button == null) return;
        if (!tabButtons.Contains(button))
            tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        if (button == null) return;
        ResetTabs();
        if (selectedTab == null || button != selectedTab)
        {
            if (button.background != null)
                button.background.color = tabHover;
        }
    }

    public void OnTabExit(TabButton button)
    {
        if (button == null) return;
        ResetTabs();
    }

    public void OnTabSelected(TabButton button)
    {
        if (selectedTab != null)
        {
            selectedTab.Deselect();
        }
        
        if (button == null) return;

        selectedTab = button;

        selectedTab.Select();

        ResetTabs();

        if (button.background != null)
            button.background.color = tabActive;

        int index = tabButtons.IndexOf(button);
        for (int i = 0; i < objectsToSwap.Count; i++)
        {
            if (objectsToSwap[i] == null) continue;
            objectsToSwap[i].SetActive(i == index);
        }
    }

    public void ResetTabs()
    {
        foreach (var button in tabButtons)
        {
            if (button == null) continue;
            if (selectedTab != null && button == selectedTab) continue;

            if (button.background != null)
                button.background.color = tabIdle;
        }
    }
}
