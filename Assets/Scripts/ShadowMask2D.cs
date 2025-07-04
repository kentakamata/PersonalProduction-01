using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShadowMask2D : MonoBehaviour
{
    [Header("基本設定")]
    public float personalViewRadius = 5f;
    public float outerRadius = 30f;
    [Range(3, 360)]
    public int resolution = 100;
    public LayerMask wallMask;

    [Header("共有タグ設定")]
    public string itemTag = "Item";

    [Header("描画設定")]
    public string sortingLayerName = "Default";
    public int sortingOrder = -10;

    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh { name = "ShadowMask2D Mesh" };
        GetComponent<MeshFilter>().mesh = mesh;

        var renderer = GetComponent<MeshRenderer>();
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = sortingOrder;
    }

    void LateUpdate()
    {
        GenerateCombinedMask();
    }

    void GenerateCombinedMask()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Vector3 center = transform.InverseTransformPoint(transform.position);
        float angleStep = 360f / resolution;

        // 外周
        for (int i = 0; i <= resolution; i++)
        {
            float rad = Mathf.Deg2Rad * i * angleStep;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
            vertices.Add(center + dir * outerRadius);
        }

        int innerStart = vertices.Count;

        // 内周（プレイヤー視界＋共有視界）
        for (int i = 0; i <= resolution; i++)
        {
            float rad = Mathf.Deg2Rad * i * angleStep;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector3 best = transform.position + dir * 0.1f;
            float maxVisible = 0f;

            // ① プレイヤー自身の視界
            EvaluateSource(transform.position, personalViewRadius, dir, ref best, ref maxVisible);

            // ② 共有可能なアイテム視界（プレイヤーが近い場合）
            foreach (GameObject item in GameObject.FindGameObjectsWithTag(itemTag))
            {
                ItemVision iv = item.GetComponent<ItemVision>();
                if (iv == null) continue;

                float dist = Vector2.Distance(transform.position, item.transform.position);
                if (dist <= iv.shareRadius)
                {
                    EvaluateSource(item.transform.position, iv.visionRadius, dir, ref best, ref maxVisible);
                }
            }

            vertices.Add(transform.InverseTransformPoint(best));
        }

        // 三角形構築
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

    /// <summary>
    /// レイを飛ばして視界端点を決定
    /// </summary>
    void EvaluateSource(Vector3 origin, float radius, Vector3 dir, ref Vector3 best, ref float maxDist)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, radius, wallMask);
        Vector3 end = hit ? hit.point : origin + dir * radius;

        float dist = Vector3.Distance(transform.position, end);
        if (dist > maxDist)
        {
            best = end;
            maxDist = dist;
        }
    }
}
