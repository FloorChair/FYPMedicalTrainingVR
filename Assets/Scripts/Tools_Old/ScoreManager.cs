using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Panel")]
    public TextMeshProUGUI totalScoreText;

    private int totalScore = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddScore(int amount)
    {
        totalScore += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (totalScoreText != null)
            totalScoreText.text = $"Total Score: {totalScore}";
    }

    public int GetTotalScore() => totalScore;

    public void ResetScore()
    {
        totalScore = 0;
        UpdateScoreText();
    }
}
