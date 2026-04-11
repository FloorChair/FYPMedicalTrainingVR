using UnityEngine;
using TMPro;

public class ResultsDisplay : MonoBehaviour
{
    public TMP_Text resultsText;
    public string fileName = "procedureResult";

    void OnEnable()
    {
        ProcedureResults.fileName = fileName;
        resultsText.text = ProcedureResults.GetFormattedResults();
    }
}