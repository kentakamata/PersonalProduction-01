using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EnemyPatrol : MonoBehaviour
{
    public List<Transform> patrolPoints;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public Transform player;
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask obstacleMask;

    private int currentPointIndex = 0;
    private bool isChasing = false;
    private Vector3 currentDirection = Vector3.right; // 進行方向の初期値

    private LineRenderer visionRenderer;

    void Start()
    {
        visionRenderer = GetComponent<LineRenderer>();
        visionRenderer.positionCount = 0;
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

        DrawView();
    }

    void Patrol()
    {
        if (patrolPoints.Count == 0) return;

        Vector3 target = patrolPoints[currentPointIndex].position;
        MoveTowards(target, patrolSpeed);

        if (Vector3.Distance(transform.position, target) < 0.2f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position).normalized;
        currentDirection = dir; // 進行方向を保存
        transform.position += dir * speed * Time.deltaTime;
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

    void DrawView()
    {
        int steps = 30;
        float stepAngle = viewAngle / steps;
        visionRenderer.positionCount = steps + 1;
        visionRenderer.startWidth = 0.05f;
        visionRenderer.endWidth = 0.05f;

        for (int i = 0; i <= steps; i++)
        {
            float angleOffset = -viewAngle / 2 + stepAngle * i;
            Vector3 dir = DirFromAngle(angleOffset);
            Vector3 endPoint = transform.position + dir * viewRadius;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask);
            if (hit.collider != null)
            {
                endPoint = hit.point;
            }

            visionRenderer.SetPosition(i, endPoint);
        }
    }

    Vector3 DirFromAngle(float angleOffset)
    {
        float baseAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        float finalAngle = baseAngle + angleOffset;
        float rad = Mathf.Deg2Rad * finalAngle;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}