using UnityEngine;
using TMPro;

[RequireComponent(typeof(ToolActionController))]
public class ToolScoring : MonoBehaviour
{
    public Transform toolTip;

    [Header("Enabled Metrics")]
    public bool scoreTime = true;
    public bool scoreSteadiness = true;
    public bool scoreAccuracy = true;
    public bool scoreConservation = true;

    [Tooltip("Allowed time for full marks (seconds)")]
    public float maxAllowedTime = 10f;

    private float startTime;
    private float elapsedTime;

    [Tooltip("Local axis that represents the needle direction")]
    public Vector3 needleLocalDirection = Vector3.forward;

    [Tooltip("Maximum allowed angular deviation (degrees)")]
    public float maxAllowedDeviation = 15f;

    [Tooltip("Ignore tiny deviations (degrees)")]
    public float deadzone = 0.5f;

    private Vector3 startNeedleDirectionWorld;
    private float maxDeviation;

    private int framesInsideZone;
    private int framesTotal;

    public enum AccuracyMode
    {
        PointTarget,
        LineInsideZone,
        MultiplePoint,
        BoundZone
    }

    public AccuracyMode accuracyMode = AccuracyMode.PointTarget;

    [Tooltip("The point or guide the tool should hit")]
    public Transform targetGuide;

    [Tooltip("Maximum distance for full accuracy (meters)")]
    public float maxAllowedDistance = 0.01f;

    [Tooltip("The ideal number of trigger presses for full marks")]
    public int idealActionCount = 1;

    [Tooltip("Number of extra presses at which the score reaches zero")]
    public int maxExtraActions = 5;

    private int actionCount;

    public TMP_Text resultsText;

    [Tooltip("Custom label to display above results (e.g., Step Name)")]
    public string stepLabel = "";

    public ProcedureGenerator procedureGenerator;

    private ToolActionController toolController;
    private bool scoringActive;
    private bool actionOngoing;

    // Print variables
    private float pickupTime;
    private float triggerPressTime;
    private float totalTriggerHeldTime;

    private void Awake()
    {
        toolController = GetComponent<ToolActionController>();

        toolController.onPickup.AddListener(BeginScoring);
        toolController.onTriggerPressed.AddListener(BeginActiveUse);
        toolController.onTriggerReleased.AddListener(EndActiveUse);

        if (resultsText == null)
            resultsText = GetComponentInChildren<TMP_Text>();
    }

    private void BeginScoring()
    {
        scoringActive = true;
        actionOngoing = false;
        actionCount = 0;
        pickupTime = Time.time;
        totalTriggerHeldTime = 0f;

        if (resultsText != null)
            resultsText.text = "";

        if (scoreTime)
            startTime = Time.time;

        if (scoreSteadiness)
            maxDeviation = 0f;

    }

    private void BeginActiveUse()
    {
        actionOngoing = true;
        triggerPressTime = Time.time;

        if (scoreConservation)
            actionCount++;

        if (scoreSteadiness)
            startNeedleDirectionWorld = transform.TransformDirection(needleLocalDirection).normalized;
    }

    private void EndActiveUse()
    {
        actionOngoing = false;
        totalTriggerHeldTime += Time.time - triggerPressTime;
    }

    private void Update()
    {
        if (!scoringActive || !actionOngoing)
            return;

        if (scoreSteadiness)
        {
            Vector3 currentNeedleDir = transform.TransformDirection(needleLocalDirection).normalized;
            float deviation = Vector3.Angle(startNeedleDirectionWorld, currentNeedleDir);
            if (deviation < deadzone) deviation = 0f;
            if (deviation > maxDeviation) maxDeviation = deviation;
        }

        if (scoreAccuracy && accuracyMode == AccuracyMode.BoundZone)
        {
            framesTotal++;
            if (IsInsideAnyGuide(toolTip != null ? toolTip.position : toolController.transform.position))
                framesInsideZone++;
        }
    }

    public void OnActionCompleted()
    {
        if (!scoringActive) return;
        scoringActive = false;

        if (scoreTime)
            elapsedTime = Time.time - startTime;

        float timeHeld = Time.time - pickupTime;
        float finalScore = FinalScore();

        ToolResult toolResult = new ToolResult
        {
            toolName = gameObject.name,
            timeHeld = timeHeld,
            triggerHeldTime = totalTriggerHeldTime,
            score = finalScore,
            rank = ScoreUtility.GetRank(finalScore)
        };

        ProcedureResults.AddToolResult(toolResult);
        DisplayResults();
    }

    private float TimeScore()
    {
        if (elapsedTime <= maxAllowedTime)
            return 100f;

        return Mathf.Clamp01(1f - ((elapsedTime - maxAllowedTime) / maxAllowedTime)) * 100f;
    }

    private float SteadinessScore()
    {
        return Mathf.Clamp01(1f - (maxDeviation / maxAllowedDeviation)) * 100f;
    }

    private float ConservationScore()
    {
        int extraActions = actionCount - idealActionCount;

        if (extraActions <= 0)
            return 100f;

        return Mathf.Clamp01(1f - ((float)extraActions / maxExtraActions)) * 100f;
    }

    private float AccuracyScore()
    {
        if (!scoreAccuracy)
            return 0f;

        if (accuracyMode == AccuracyMode.PointTarget)
        {
            if (targetGuide == null)
                return 0f;

            float distance = Vector3.Distance(toolController.transform.position, targetGuide.position);
            return Mathf.Clamp01(1f - (distance / maxAllowedDistance)) * 100f;
        }

        if (accuracyMode == AccuracyMode.LineInsideZone)
        {
            Vector3 start = toolController.LastDrawStartPoint;
            Vector3 end = toolController.LastDrawEndPoint;

            float distance = Vector3.Distance(start, end);
            if (distance <= 0.0001f)
                return 0f;

            int steps = 30;
            int insideCount = 0;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 point = Vector3.Lerp(start, end, t);

                if (IsInsideAnyGuide(point))
                    insideCount++;
            }

            float percentageInside = insideCount / (float)(steps + 1);
            return percentageInside * 100f;
        }

        if (accuracyMode == AccuracyMode.MultiplePoint)
        {
            Vector3 start = toolController.LastDrawStartPoint;
            Vector3 end = toolController.LastDrawEndPoint;

            int totalGuides = 0;
            int correctPoints = 0;

            foreach (var guide in toolController.associatedGuides)
            {
                if (guide == null || !guide.activeInHierarchy)
                    continue;

                Collider col = guide.GetComponent<Collider>();
                if (col == null)
                    continue;

                totalGuides++;

                if (Vector3.Distance(col.ClosestPoint(start), start) <= maxAllowedDistance)
                    correctPoints++;

                if (Vector3.Distance(col.ClosestPoint(end), end) <= maxAllowedDistance)
                    correctPoints++;
            }

            if (totalGuides == 0)
                return 0f;

            int totalPossiblePoints = totalGuides * 2;
            return (correctPoints / (float)totalPossiblePoints) * 100f;
        }

        if (accuracyMode == AccuracyMode.BoundZone)
        {
            if (framesTotal == 0)
                return 0f;

            return ((float)framesInsideZone / framesTotal) * 100f;
        }

        return 0f;
    }

    private float FinalScore()
    {
        float total = 0f;
        int count = 0;

        if (scoreTime) { total += TimeScore(); count++; }
        if (scoreSteadiness) { total += SteadinessScore(); count++; }
        if (scoreAccuracy) { total += AccuracyScore(); count++; }
        if (scoreConservation) { total += ConservationScore(); count++; }

        return count > 0 ? total / count : 0f;
    }

    private void DisplayResults()
    {
        if (resultsText == null)
            return;

        string output = "";

        if (!string.IsNullOrEmpty(stepLabel))
            output += $"{stepLabel}\n\n";

        if (scoreTime)
            output += $"Time Score: {ScoreUtility.GetRank(TimeScore())} ({TimeScore():F1}%)\n";

        if (scoreSteadiness)
            output += $"Steadiness Score: {ScoreUtility.GetRank(SteadinessScore())} ({SteadinessScore():F1}%)\n";

        if (scoreAccuracy)
        {
            float acc = AccuracyScore();
            output += $"Accuracy Score: {ScoreUtility.GetRank(acc)} ({acc:F1}%)\n";

            if (accuracyMode == AccuracyMode.BoundZone)
                output += $"Frames in zone: {framesInsideZone}/{framesTotal}\n";
        }

        if (scoreConservation)
        {
            float con = ConservationScore();
            output += $"Conservation Score: {ScoreUtility.GetRank(con)} ({con:F1}%)\n";
            output += $"Actions Used: {actionCount} (ideal: {idealActionCount})\n";
        }

        output += $"\n<b>Final Score: {ScoreUtility.GetRank(FinalScore())} ({FinalScore():F1}%)</b>";

        resultsText.text = output;

        if (procedureGenerator != null)
            procedureGenerator.ReportStepScore(FinalScore());
    }

    private bool IsInsideAnyGuide(Vector3 point)
    {
        foreach (var guide in toolController.associatedGuides)
        {
            if (guide == null || !guide.activeInHierarchy)
                continue;

            Collider col = guide.GetComponent<Collider>();
            if (col == null)
                continue;

            Vector3 closest = col.ClosestPoint(point);

            if (Vector3.Distance(closest, point) < 0.001f)
                return true;
        }

        return false;
    }
}