using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ItemVisionMask : MonoBehaviour
{
    [Header("視界設定")]
    public float revealRadius = 4f;
    [Range(3, 360)] public int resolution = 60;
    public LayerMask wallMask;

    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh { name = "ItemVisionMask Mesh" };
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void LateUpdate()
    {
        GenerateMask();
    }

    void GenerateMask()
    {
        Vector3 center = transform.InverseTransformPoint(transform.position);
        float angleStep = 360f / resolution;

        // 頂点と三角形データ
        Vector3[] vertices = new Vector3[resolution + 2];
        int[] triangles = new int[resolution * 3];

        vertices[0] = center;

        for (int i = 0; i <= resolution; i++)
        {
            float rad = Mathf.Deg2Rad * i * angleStep;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));

            // Raycastで壁に遮られた距離まで描画
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, revealRadius, wallMask);
            Vector3 end = hit ? hit.point : transform.position + dir * revealRadius;

            vertices[i + 1] = transform.InverseTransformPoint(end);
        }

        for (int i = 0; i < resolution; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
