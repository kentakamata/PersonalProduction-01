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

        // �O���h�[�i�c�̉~�������i�O���~�j
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = DirFromAngle(angle);
            vertices.Add(dir * outerRadius);
        }

        int innerOffset = vertices.Count;

        // ���E�̓��~�iraycast ���ʁj�����
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = DirFromAngle(angle);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, wallMask);

            Vector3 hitPointWorld = hit ? (Vector3)hit.point : transform.position + dir * viewRadius;
            Vector3 local = transform.InverseTransformPoint(hitPointWorld);
            vertices.Add(local);
        }

        // �O�p�`���Ȃ��i�O�� �� �����j
        for (int i = 0; i < rayCount; i++)
        {
            int outerA = i;
            int outerB = i + 1;
            int innerA = innerOffset + i;
            int innerB = innerOffset + i + 1;

            // �O�����̋t�O�p�`
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