using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Suture : Tool
{
    [Header("VR Input")]
    public InputActionProperty stitchAction;

    [Header("Stitch Settings")]
    public GameObject stitchesPrefab;
    public Transform stitchesParent;
    public List<StitchMarker> markers;
    public List<Vector2Int> stitchPairs;

    [Header("UI")]
    public TextMeshProUGUI scoreText; // optional, assign in inspector
    public GameObject suturePanel;    // panel to show only while holding

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isHeld = false;
    private bool isComplete = false;
    private StitchMarker currentMarker = null;
    private StitchMarker startMarker = null;

    private HashSet<int> stitchedMarkers = new HashSet<int>();
    private HashSet<Vector2Int> stitchedPairs = new HashSet<Vector2Int>();

    private int runningScore = 0;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnPickup);
            grabInteractable.selectExited.AddListener(OnDrop);
        }

        if (suturePanel != null)
            suturePanel.SetActive(false);
    }

    private void OnPickup(SelectEnterEventArgs args)
    {
        isHeld = true;
        SetMarkersActive(true);
        UpdateScoreText();

        if (suturePanel != null)
            suturePanel.SetActive(true); // show panel while held
    }

    private void OnDrop(SelectExitEventArgs args)
    {
        isHeld = false;
        SetMarkersActive(false);
        currentMarker = null;
        startMarker = null;

        if (suturePanel != null)
            suturePanel.SetActive(false); // hide panel when dropped
    }

    private void OnTriggerEnter(Collider other) => TryApply<StitchMarker>(other);
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<StitchMarker>() == currentMarker)
            currentMarker = null;
    }

    private void Update()
    {
        if (!isHeld || currentMarker == null || isComplete) return;

        if (stitchAction.action.WasPressedThisFrame() && startMarker == null)
        {
            startMarker = currentMarker;
        }

        if (stitchAction.action.WasReleasedThisFrame() && startMarker != null)
        {
            if (currentMarker != null && currentMarker != startMarker)
            {
                int startIndex = markers.IndexOf(startMarker);
                int endIndex = markers.IndexOf(currentMarker);

                foreach (var pair in stitchPairs)
                {
                    if ((pair.x == startIndex && pair.y == endIndex) ||
                        (pair.y == startIndex && pair.x == endIndex))
                    {
                        SpawnStitchBetweenMarkers(startIndex, endIndex);

                        startMarker.MarkStitched();
                        currentMarker.MarkStitched();

                        stitchedMarkers.Add(startIndex);
                        stitchedMarkers.Add(endIndex);
                        stitchedPairs.Add(pair);

                        runningScore++;
                        ScoreManager.Instance.AddScore(runningScore * 100);
                        UpdateScoreText();
                        break;
                    }
                }

                if (stitchedPairs.Count >= stitchPairs.Count)
                    CompleteSuturing();
            }

            startMarker = null;
        }
    }

    private void CompleteSuturing()
    {
        if (isComplete) return;
        isComplete = true;

        HealGum();

        // Show final results panel
        ToolPanelManager panelManager = FindFirstObjectByType<ToolPanelManager>();
        if (panelManager != null)
            panelManager.ShowFinalResults();
    }

    private void SpawnStitchBetweenMarkers(int indexA, int indexB)
    {
        if (stitchesPrefab == null || stitchesParent == null) return;

        Vector3 posA = markers[indexA].transform.position;
        Vector3 posB = markers[indexB].transform.position;
        Vector3 center = (posA + posB) * 0.5f;
        Quaternion rotation = Quaternion.LookRotation(posB - posA);

        GameObject stitch = Instantiate(stitchesPrefab, center, rotation, stitchesParent);
        Vector3 scale = stitch.transform.localScale;
        scale.z = Vector3.Distance(posA, posB);
        stitch.transform.localScale = scale;
    }

    private void SetMarkersActive(bool active)
    {
        if (markers == null) return;
        foreach (var marker in markers)
            if (marker != null)
                marker.gameObject.SetActive(active);
    }

    private void HealGum()
    {
        GumBehaviour gum = FindFirstObjectByType<GumBehaviour>();
        gum?.Heal();
    }

    protected override void OnAction<T>(T target)
    {
        currentMarker = target as StitchMarker;
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = $"Stitches Placed: {runningScore}";
    }
}
