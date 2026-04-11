using UnityEngine;

public static class ScoreUtility
{
    public static string GetRank(float score)
    {
        if (score > 90f) return "Excellent";
        if (score > 75f) return "High";
        if (score > 60f) return "Average";
        if (score > 50f) return "Passable";
        return "Fail";
    }
}
