using UnityEngine;
using System.Collections.Generic;

public class MarkerHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow;
    public float highlightDistance = 0.0001f; // distance from tool to highlight

    [Header("Tools")]
    public List<Transform> toolTips; // assign the tip transforms of all tools in the scene

    private Renderer rend;
    private Color originalColor; // store the original color
    private bool isHighlighted = false;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color; // remember original color
        }
    }

    private void Update()
    {
        if (rend == null) return;

        bool nearTool = false;

        foreach (var tip in toolTips)
        {
            if (tip == null) continue;

            float dist = Vector3.Distance(transform.position, tip.position);
            if (dist <= highlightDistance)
            {
                nearTool = true;
                break;
            }
        }

        // Change color based on proximity
        if (nearTool && !isHighlighted)
        {
            rend.material.color = highlightColor;
            isHighlighted = true;
        }
        else if (!nearTool && isHighlighted)
        {
            rend.material.color = originalColor; // restore original color
            isHighlighted = false;
        }
    }
}
