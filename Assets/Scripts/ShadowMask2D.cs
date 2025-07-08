using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlayerVisionMask : MonoBehaviour
{
    public float viewRadius = 5f;
    public float outerRadius = 30f;
    [Range(3, 360)] public int resolution = 100;
    public LayerMask wallMask;

    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh { name = "PlayerVisionMask Mesh" };
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void LateUpdate()
    {
        GenerateMask();
    }

    void GenerateMask()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Vector3 center = transform.InverseTransformPoint(transform.position);
        float angleStep = 360f / resolution;

        for (int i = 0; i <= resolution; i++)
        {
            float rad = Mathf.Deg2Rad * i * angleStep;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
            vertices.Add(center + dir * outerRadius);
        }

        int innerStart = vertices.Count;

        for (int i = 0; i <= resolution; i++)
        {
            float rad = Mathf.Deg2Rad * i * angleStep;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, wallMask);
            Vector3 end = hit ? hit.point : transform.position + dir * viewRadius;

            vertices.Add(transform.InverseTransformPoint(end));
        }

        for (int i = 0; i < resolution; i++)
        {
            int outerA = i;
            int outerB = i + 1;
            int innerA = innerStart + i;
            int innerB = innerStart + i + 1;

            triangles.Add(outerA); triangles.Add(outerB); triangles.Add(innerB);
            triangles.Add(outerA); triangles.Add(innerB); triangles.Add(innerA);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
    }
}
