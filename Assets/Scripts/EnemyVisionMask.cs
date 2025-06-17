using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EnemyVisionMask : MonoBehaviour
{
    [Header("視野設定")]
    public LayerMask obstacleMask; // 障害物
    public float viewRadius = 8f;
    public float viewAngle = 90f;
    public int rayCount = 90;
    public float outerRadius = 50f;

    [Header("ターゲットと向き")]
    public Transform target; // プレイヤーなど
    public Vector2 forward = Vector2.right; // 向き

    [Header("視野マテリアル")]
    [SerializeField] private Material visionMaterial; // ここでマテリアルを設定

    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Enemy Vision Mesh";
        GetComponent<MeshFilter>().mesh = mesh;

        if (visionMaterial != null)
        {
            GetComponent<MeshRenderer>().material = visionMaterial;
        }
    }

    void LateUpdate()
    {
        GenerateVisionMesh();
    }

    void GenerateVisionMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float angleStep = viewAngle / rayCount;
        float startAngle = -viewAngle / 2f;

        // 外周
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector3 dir = DirFromAngle(angle);
            vertices.Add(dir * outerRadius);
        }

        int innerOffset = vertices.Count;

        // 内周（視認可能範囲）
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector3 dir = DirFromAngle(angle);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask);
            Vector3 worldPoint = hit ? (Vector3)hit.point : transform.position + dir * viewRadius;
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            vertices.Add(local);
        }

        for (int i = 0; i < rayCount; i++)
        {
            int outerA = i;
            int outerB = i + 1;
            int innerA = innerOffset + i;
            int innerB = innerOffset + i + 1;

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

    public bool IsInVision(Vector3 worldPos)
    {
        if (mesh == null || mesh.vertexCount == 0) return false;

        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        return !PointInPolygon(localPos, mesh.vertices);
    }

    public bool CanSeeTarget()
    {
        if (target == null) return false;
        if (!IsInVision(target.position)) return false;

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angle = Vector2.Angle(forward, dirToTarget);
        if (angle > viewAngle / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToTarget, viewRadius, obstacleMask);
        return hit.collider == null || hit.collider.transform == target;
    }

    Vector3 DirFromAngle(float angle)
    {
        float rad = Mathf.Deg2Rad * angle;
        Vector2 baseDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Quaternion rot = Quaternion.FromToRotation(Vector2.right, forward);
        return rot * baseDir;
    }

    bool PointInPolygon(Vector2 point, Vector3[] polygon)
    {
        int count = polygon.Length;
        bool inside = false;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            Vector2 pi = new Vector2(polygon[i].x, polygon[i].y);
            Vector2 pj = new Vector2(polygon[j].x, polygon[j].y);

            if (((pi.y > point.y) != (pj.y > point.y)) &&
                (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y + Mathf.Epsilon) + pi.x))
            {
                inside = !inside;
            }
        }

        return inside;
    }
}
