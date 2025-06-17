using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("�p�g���[���ݒ�")]
    [Tooltip("�G�����񂷂�p�g���[���|�C���g�̃��X�g")]
    public List<Transform> patrolPoints;

    [Tooltip("�ʏ펞�̈ړ����x")]
    public float patrolSpeed = 2f;

    [Tooltip("�v���C���[�ǐՎ��̈ړ����x")]
    public float chaseSpeed = 4f;

    [Tooltip("�p�g���[���|�C���g���B���ɒ�~����b��")]
    public float stopDuration = 1f;

    [Tooltip("������ς���X�s�[�h")]
    public float turnSpeed = 5f;

    [Header("����ݒ�")]
    [Tooltip("�v���C���[��Transform")]
    public Transform player;

    [Tooltip("�G�̎��씼�a")]
    public float viewRadius = 5f;

    [Tooltip("����̊p�x�i�x�j")]
    [Range(0, 360)]
    public float viewAngle = 90f;

    [Tooltip("���상�b�V���̕�����")]
    public int rayCount = 60;

    [Tooltip("������Ղ郌�C���[")]
    public LayerMask obstacleMask;

    [Header("���상�b�V��")]
    [Tooltip("���상�b�V����`�悷�� GameObject�iMeshRenderer �t���j")]
    public GameObject visionMeshObject;

    [Tooltip("���상�b�V���̃}�e���A���iUnlit �Ȃǁj")]
    public Material visionMaterial;

    private int currentPointIndex = 0;
    private bool isChasing = false;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    private Vector3 currentDirection = Vector3.right;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        if (visionMeshObject == null)
        {
            Debug.LogError("visionMeshObject ��ݒ肵�Ă��������B");
            enabled = false;
            return;
        }

        meshFilter = visionMeshObject.GetComponent<MeshFilter>() ?? visionMeshObject.AddComponent<MeshFilter>();
        meshRenderer = visionMeshObject.GetComponent<MeshRenderer>() ?? visionMeshObject.AddComponent<MeshRenderer>();

        mesh = new Mesh { name = "Enemy View Mesh" };
        meshFilter.mesh = mesh;

        if (visionMaterial != null)
            meshRenderer.material = visionMaterial;

        visionMeshObject.transform.SetParent(transform);
        visionMeshObject.transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            isChasing = true;
        }
        else if (isChasing && Vector3.Distance(transform.position, player.position) > viewRadius + 1f)
        {
            isChasing = false;
        }

        if (isChasing)
        {
            MoveTowards(player.position, chaseSpeed);
        }
        else
        {
            Patrol();
        }

        GenerateViewMesh();
    }

    void Patrol()
    {
        if (patrolPoints.Count == 0) return;

        Vector3 target = patrolPoints[currentPointIndex].position;
        float distance = Vector3.Distance(transform.position, target);

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= stopDuration)
            {
                waitTimer = 0f;
                isWaiting = false;
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
            }
            return;
        }

        if (distance < 0.2f && !isWaiting)
        {
            isWaiting = true;
            return;
        }

        Vector3 dir = (target - transform.position).normalized;
        currentDirection = Vector3.Lerp(currentDirection, dir, Time.deltaTime * turnSpeed);
        transform.position += currentDirection * patrolSpeed * Time.deltaTime;
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position).normalized;
        currentDirection = Vector3.Lerp(currentDirection, dir, Time.deltaTime * turnSpeed);
        transform.position += currentDirection * speed * Time.deltaTime;
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer > viewRadius) return false;

        float angle = Vector3.Angle(currentDirection, dirToPlayer);
        if (angle < viewAngle / 2f)
        {
            if (!Physics2D.Raycast(transform.position, dirToPlayer, distToPlayer, obstacleMask))
            {
                return true;
            }
        }

        return false;
    }

    void GenerateViewMesh()
    {
        List<Vector3> vertices = new List<Vector3> { Vector3.zero };
        List<int> triangles = new List<int>();

        float angleStep = viewAngle / rayCount;
        float startAngle = -viewAngle / 2f;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector3 dir = DirFromAngle(angle);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask);
            Vector3 hitPoint = hit ? (Vector3)hit.point : transform.position + dir * viewRadius;
            Vector3 localPoint = visionMeshObject.transform.InverseTransformPoint(hitPoint);
            vertices.Add(localPoint);
        }

        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
    }

    Vector3 DirFromAngle(float angleOffset)
    {
        float baseAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        float finalAngle = baseAngle + angleOffset;
        float rad = finalAngle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}
