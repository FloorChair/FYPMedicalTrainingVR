using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SkinnedCuttableMesh : MonoBehaviour
{
    private SkinnedMeshRenderer smr;
    private Mesh originalMesh;  // The original uncut mesh
    private Mesh workingMesh;   // The mesh we actually modify
    private bool[] removedTriangles;

    private void Awake()
    {
        smr = GetComponent<SkinnedMeshRenderer>();

        // Store a copy of the original mesh
        originalMesh = Mesh.Instantiate(smr.sharedMesh);
        originalMesh.name = $"{smr.sharedMesh.name}_Original";

        // Create working mesh from original
        workingMesh = Mesh.Instantiate(originalMesh);
        workingMesh.name = $"{smr.sharedMesh.name}_Working";
        smr.sharedMesh = workingMesh;

        removedTriangles = new bool[workingMesh.triangles.Length / 3];
    }

    public void ProcessCutAtWorldPoint(Vector3 worldPoint, float radius)
    {
        Mesh bakedMesh = new Mesh();
        smr.BakeMesh(bakedMesh);

        Vector3[] bakedVerts = bakedMesh.vertices;
        int[] tris = workingMesh.triangles;
        bool anyRemoved = false;

        for (int i = 0; i < tris.Length; i += 3)
        {
            if (removedTriangles[i / 3]) continue;
            if (tris[i] >= bakedVerts.Length || tris[i + 1] >= bakedVerts.Length || tris[i + 2] >= bakedVerts.Length) continue;

            Vector3 worldCentroid = smr.transform.TransformPoint(
                (bakedVerts[tris[i]] + bakedVerts[tris[i + 1]] + bakedVerts[tris[i + 2]]) / 3f
            );

            if (Vector3.Distance(worldCentroid, worldPoint) <= radius)
            {
                removedTriangles[i / 3] = true;
                anyRemoved = true;
            }
        }

        Destroy(bakedMesh);

        if (anyRemoved)
            RebuildMeshTriangles();
    }

    private void RebuildMeshTriangles()
    {
        int[] originalTris = originalMesh.triangles;
        var newTris = new List<int>(originalTris.Length);

        for (int i = 0; i < originalTris.Length; i += 3)
        {
            if (!removedTriangles[i / 3])
            {
                newTris.Add(originalTris[i]);
                newTris.Add(originalTris[i + 1]);
                newTris.Add(originalTris[i + 2]);
            }
        }

        workingMesh.triangles = newTris.ToArray();
        workingMesh.RecalculateBounds();
        smr.sharedMesh = workingMesh;
    }

    public void ResetMeshCuts()
    {
        // Restore working mesh from original
        workingMesh = Mesh.Instantiate(originalMesh);
        workingMesh.name = $"{smr.sharedMesh.name}_Working";
        smr.sharedMesh = workingMesh;

        // Reset all cuts
        removedTriangles = new bool[workingMesh.triangles.Length / 3];
    }
}