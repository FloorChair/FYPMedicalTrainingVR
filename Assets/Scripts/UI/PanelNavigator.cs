using UnityEngine;
using UnityEngine.UI;

public class PanelNavigator : MonoBehaviour
{
    [Header("Panels")]
    public GameObject[] panels;

    [Header("Navigation Buttons")]
    public Button leftButton;
    public Button rightButton;

    private int currentIndex = 0;

    void Start()
    {
        leftButton.onClick.AddListener(GoLeft);
        rightButton.onClick.AddListener(GoRight);

        ShowPanel(currentIndex);
    }

    void GoLeft()
    {
        currentIndex = (currentIndex - 1 + panels.Length) % panels.Length;
        ShowPanel(currentIndex);
    }

    void GoRight()
    {
        currentIndex = (currentIndex + 1) % panels.Length;
        ShowPanel(currentIndex);
    }

    void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
            panels[i].SetActive(i == index);
    }
}