using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ShadowMask2D shadowMask;
    public LayerMask wallMask;
    public float moveSpeed = 5f;
    public float stopDistanceFromWall = 0.05f;

    private Vector3 targetPosition;
    private bool moving = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        targetPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!moving && Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            if (shadowMask != null && shadowMask.IsVisible(mouseWorld))
            {
                targetPosition = mouseWorld;
                moving = true;

                if (spriteRenderer != null)
                    spriteRenderer.flipX = (mouseWorld.x < transform.position.x);
            }
        }

        if (moving)
        {
            Vector3 current = transform.position;
            Vector3 next = Vector3.MoveTowards(current, targetPosition, moveSpeed * Time.deltaTime);

            RaycastHit2D hit = Physics2D.Linecast(current, next, wallMask);
            if (hit.collider != null)
            {
                Vector3 hitPoint = hit.point;
                Vector3 direction = (hitPoint - current).normalized;
                transform.position = hitPoint - direction * stopDistanceFromWall;
                moving = false;
                return;
            }

            transform.position = next;

            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                moving = false;
            }
        }
    }
}