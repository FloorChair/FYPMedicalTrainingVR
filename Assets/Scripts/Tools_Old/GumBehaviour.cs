using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class GumBehaviour : MonoBehaviour, IMeshCuttable
{
    private Mesh mesh;
    private Vector3[] vertices;
    private List<int> triangles;

    [Header("Dependencies")]
    public ToothBehaviour linkedTooth; // assign in inspector

    [Header("Cut Settings")]
    public float cutRadius = 0.02f;   // world units

    [Header("Bounding Box Settings")]
    public bool useBoundingBox = false;
    public BoxCollider cutBox;        // assign a BoxCollider in the scene

    private Vector3[] originalVertices;
    private List<int> originalTriangles;

    private void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshCollider mc = GetComponent<MeshCollider>();

        mesh = Instantiate(mf.mesh);
        mf.mesh = mesh;

        vertices = mesh.vertices;
        triangles = new List<int>(mesh.triangles);

        // Save original for healing
        originalVertices = mesh.vertices;
        originalTriangles = new List<int>(mesh.triangles);

        if (mc != null)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
        }
    }

    private bool IsInsideBox(Vector3 worldVertex)
    {
        if (cutBox == null) return true;

        Vector3 localPos = cutBox.transform.InverseTransformPoint(worldVertex);
        Vector3 halfSize = cutBox.size * 0.5f;

        return (localPos.x >= -halfSize.x && localPos.x <= halfSize.x) &&
               (localPos.y >= -halfSize.y && localPos.y <= halfSize.y) &&
               (localPos.z >= -halfSize.z && localPos.z <= halfSize.z);
    }

    public void CutAt(Vector3 worldPosition, Vector3 deltaMovement, float radius, float pushMultiplier)
    {
        if (linkedTooth != null && !linkedTooth.isAnesthetized) return;

        float rSqr = cutRadius * cutRadius;
        List<int> trianglesToRemove = new List<int>();

        for (int i = 0; i < triangles.Count; i += 3)
        {
            Vector3 v0 = transform.TransformPoint(vertices[triangles[i]]);
            Vector3 v1 = transform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 v2 = transform.TransformPoint(vertices[triangles[i + 2]]);

            bool insideSphere = (v0 - worldPosition).sqrMagnitude < rSqr ||
                                (v1 - worldPosition).sqrMagnitude < rSqr ||
                                (v2 - worldPosition).sqrMagnitude < rSqr;

            bool insideBox = !useBoundingBox || IsInsideBox(v0) || IsInsideBox(v1) || IsInsideBox(v2);

            if (insideSphere && insideBox)
                trianglesToRemove.Add(i);
        }

        for (int i = trianglesToRemove.Count - 1; i >= 0; i--)
        {
            int index = trianglesToRemove[i];
            triangles.RemoveRange(index, 3);
        }

        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc != null)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
        }
    }

    // Restore original mesh (called after stitching)
    public void Heal()
    {
        mesh.vertices = originalVertices;
        mesh.triangles = originalTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc != null)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
        }
    }
}
