using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class GuidedScalpel : Tool
{
    [Header("VR Input")]
    public InputActionProperty cutAction;

    [Header("Cut Settings")]
    public float cutRadius = 0.02f;
    public List<CutMarker> markers;
    public List<Vector2Int> cutPairs;   // define marker index pairs for cuts

    [Header("UI (Scalpel Panel)")]
    public TextMeshProUGUI scalpelText; // assign in Inspector
    public GameObject scalpelPanel;     // assign the full panel object

    [Header("Scalpel Tip")]
    public Transform scalpelTip;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isHeld = false;
    private CutMarker currentMarker = null;

    private IMeshCuttable gumTarget;
    private CutMarker startMarker = null;

    // For scoring
    private List<Vector3> cutSamples = new List<Vector3>();
    private float lastSampleTime = 0f;
    private float sampleInterval = 0.02f;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnPickup);
            grabInteractable.selectExited.AddListener(OnDrop);
        }

        if (scalpelPanel != null)
            scalpelPanel.SetActive(false);

        UpdatePanelText("Ready");
    }

    private void OnPickup(SelectEnterEventArgs args)
    {
        isHeld = true;
        SetMarkersActive(true);
        gumTarget = FindFirstObjectByType<GumBehaviour>();

        if (scalpelPanel != null)
            scalpelPanel.SetActive(true);

        UpdatePanelText("Scalpel picked up");
    }

    private void OnDrop(SelectExitEventArgs args)
    {
        isHeld = false;
        SetMarkersActive(false);
        currentMarker = null;
        startMarker = null;
        cutSamples.Clear();

        if (scalpelPanel != null)
            scalpelPanel.SetActive(false);

        UpdatePanelText("Ready");
    }

    private void OnTriggerEnter(Collider other) => TryApply<CutMarker>(other);
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CutMarker>() == currentMarker)
            currentMarker = null;
    }

    private void Update()
    {
        if (!isHeld) return;

        // Only show marker feedback if NOT holding trigger
        //if (!cutAction.action.IsPressed() && currentMarker != null)
        //{
        //    bool inside = IsScalpelInsideMarker(currentMarker);
        //    UpdatePanelText(inside ? "Scalpel inside marker" : "Scalpel outside marker");
        //}

        // Collect cut samples while holding trigger
        if (cutAction.action.IsPressed() && startMarker != null)
        {
            if (Time.time - lastSampleTime > sampleInterval)
            {
                cutSamples.Add(scalpelTip.position);
                lastSampleTime = Time.time;
            }
        }

        // Start cut
        if (cutAction.action.WasPressedThisFrame() && startMarker == null)
        {
            startMarker = currentMarker;
            startMarker?.MarkCut();
            cutSamples.Clear();
        }

        // End cut
        if (cutAction.action.WasReleasedThisFrame() && startMarker != null)
        {
            if (currentMarker != null && currentMarker != startMarker)
            {
                int startIndex = markers.IndexOf(startMarker);
                int endIndex = markers.IndexOf(currentMarker);

                foreach (var pair in cutPairs)
                {
                    if ((pair.x == startIndex && pair.y == endIndex) ||
                        (pair.y == startIndex && pair.x == endIndex))
                    {
                        // Perform the cut along the markers
                        PerformCutBetweenMarkers(startIndex, endIndex);
                        currentMarker.MarkCut();

                        // Evaluate cut score
                        float score = EvaluateCut(startMarker, currentMarker, cutSamples);
                        int intScore = Mathf.RoundToInt(score);
                        ScoreManager.Instance.AddScore(intScore);
                        UpdatePanelText($"Cut complete\nScore: {score:F1}%");

                        cutSamples.Clear();
                        break;
                    }
                }
            }

            startMarker = null;
        }
    }

    private bool IsScalpelInsideMarker(CutMarker marker)
    {
        if (marker == null || scalpelTip == null) return false;

        Collider col = marker.GetComponent<Collider>();
        if (col == null) return false;

        return col.bounds.Contains(scalpelTip.position);
    }

    private void PerformCutBetweenMarkers(int indexA, int indexB)
    {
        if (gumTarget == null) return;

        Vector3 posA = markers[indexA].transform.position;
        Vector3 posB = markers[indexB].transform.position;

        int steps = Mathf.CeilToInt(Vector3.Distance(posA, posB) / (cutRadius * 0.5f));

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 point = Vector3.Lerp(posA, posB, t);
            gumTarget.CutAt(point, Vector3.zero, cutRadius, 1f);
        }
    }

    private float EvaluateCut(CutMarker start, CutMarker end, List<Vector3> samples)
    {
        if (samples.Count < 2) return 0f;

        Vector3 pA = start.transform.position;
        Vector3 pB = end.transform.position;
        Vector3 dir = (pB - pA).normalized;
        float allowedRadius = cutRadius * 1.5f;

        int inRegionCount = 0;
        float totalDist = 0f;

        foreach (var sample in samples)
        {
            Vector3 proj = pA + Vector3.Project(sample - pA, pB - pA);
            float dist = Vector3.Distance(sample, proj);
            totalDist += dist;

            if (dist <= allowedRadius)
                inRegionCount++;
        }

        float meanDist = totalDist / samples.Count;
        float pathAccuracy = Mathf.Clamp01(1f - (meanDist / (allowedRadius * 2f)));
        float continuity = (float)inRegionCount / samples.Count;

        // Smoothness
        float smoothness = 1f;
        if (samples.Count > 2)
        {
            List<float> speeds = new List<float>();
            for (int i = 1; i < samples.Count; i++)
                speeds.Add(Vector3.Distance(samples[i], samples[i - 1]) / sampleInterval);

            float avg = 0f;
            foreach (float s in speeds) avg += s;
            avg /= speeds.Count;

            float variance = 0f;
            foreach (float s in speeds) variance += Mathf.Pow(s - avg, 2);
            variance /= speeds.Count;

            smoothness = 1f - Mathf.Clamp01(variance / (avg * avg + 1e-5f));
        }

        float score = 100f * (0.5f * pathAccuracy + 0.3f * continuity + 0.2f * smoothness);
        return score;
    }

    private void SetMarkersActive(bool active)
    {
        if (markers == null) return;

        foreach (var marker in markers)
        {
            if (marker != null)
                marker.gameObject.SetActive(active);
        }
    }

    protected override void OnAction<T>(T target)
    {
        currentMarker = target as CutMarker;
    }

    private void UpdatePanelText(string text)
    {
        if (scalpelText != null)
            scalpelText.text = text;
    }
}
