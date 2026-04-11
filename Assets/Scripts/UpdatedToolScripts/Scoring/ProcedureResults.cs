using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class ProcedureResult
{
    public float overallScore;
    public string rank;
    public List<ToolResult> toolResults = new List<ToolResult>();
}

[System.Serializable]
public class ToolResult
{
    public string toolName;
    public float timeHeld; 
    public float triggerHeldTime; 
    public float score;
    public string rank;
}

[System.Serializable]
public class ProcedureResultList
{
    public List<ProcedureResult> results = new List<ProcedureResult>();
}

public static class ProcedureResults
{
    public static string fileName = "procedureResult";

    private static string savePath => Path.Combine(Application.persistentDataPath, $"{fileName}.json");

    private static List<ToolResult> pendingToolResults = new List<ToolResult>();

    public static void ClearPending() => pendingToolResults.Clear();

    public static void AddToolResult(ToolResult result)
    {
        pendingToolResults.Add(result);
    }

    public static void Save(float score, string rank)
    {
        ProcedureResultList resultList;

        if (File.Exists(savePath))
            resultList = JsonUtility.FromJson<ProcedureResultList>(File.ReadAllText(savePath));
        else
            resultList = new ProcedureResultList();

        resultList.results.Add(new ProcedureResult
        {
            overallScore = score,
            rank = rank,
            toolResults = new List<ToolResult>(pendingToolResults)
        });

        File.WriteAllText(savePath, JsonUtility.ToJson(resultList, true));
        pendingToolResults.Clear();
        Debug.Log($"Result saved to {savePath}");
    }

    public static ProcedureResultList Load()
    {
        if (!File.Exists(savePath))
            return new ProcedureResultList();

        return JsonUtility.FromJson<ProcedureResultList>(File.ReadAllText(savePath));
    }

    public static string GetFormattedResults()
    {
        ProcedureResultList resultList = Load();

        if (resultList.results.Count == 0)
            return "No results yet.";

        resultList.results.Sort((a, b) => b.overallScore.CompareTo(a.overallScore));

        string output = "<b>Results</b>\n\n";
        for (int i = 0; i < resultList.results.Count; i++)
        {
            ProcedureResult r = resultList.results[i];
            output += $"{i + 1}. {r.overallScore:F1}% - {r.rank}\n";
        }

        return output;
    }
}