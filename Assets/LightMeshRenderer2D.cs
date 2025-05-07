using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShadowMask2D : MonoBehaviour
{
    public LayerMask wallMask;
    public float viewRadius = 8f;
    public int rayCount = 360;
    public float outerRadius = 50f;

    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Shadow Mask Mesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void LateUpdate()
    {
        GenerateShadowMesh();
    }

    void GenerateShadowMesh()
    {

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float angleStep = 360f / rayCount;

        // 外側ドーナツの円周を作る（外周円）
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = DirFromAngle(angle);
            vertices.Add(dir * outerRadius);
        }

        int innerOffset = vertices.Count;

        // 視界の内円（raycast 結果）を作る
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = DirFromAngle(angle);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, wallMask);

            Vector3 hitPointWorld = hit ? (Vector3)hit.point : transform.position + dir * viewRadius;
            Vector3 local = transform.InverseTransformPoint(hitPointWorld);
            vertices.Add(local);
        }

        // 三角形をつなぐ（外周 → 内周）
        for (int i = 0; i < rayCount; i++)
        {
            int outerA = i;
            int outerB = i + 1;
            int innerA = innerOffset + i;
            int innerB = innerOffset + i + 1;

            // 外→内の逆三角形
            triangles.Add(outerA);
            triangles.Add(outerB);
            triangles.Add(innerB);

            triangles.Add(outerA);
            triangles.Add(innerB);
            triangles.Add(innerA);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

    }

    Vector3 DirFromAngle(float angle)
    {
        float rad = Mathf.Deg2Rad * angle;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}