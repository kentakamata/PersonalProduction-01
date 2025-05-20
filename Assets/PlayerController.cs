using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ShadowMask2D shadowMask; // 可視範囲を参照
    public float moveSpeed = 5f;

    private Vector3 targetPosition;
    private bool moving = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        targetPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>(); // スプライトがある場合の取得
    }

    void Update()
    {
        // 移動中でないときだけクリック受付
        if (!moving && Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            // 視界内か判定して移動許可
            if (shadowMask != null && shadowMask.IsVisible(mouseWorld))
            {
                targetPosition = mouseWorld;
                moving = true;

                // クリック方向にスプライトを反転（左右）
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (mouseWorld.x < transform.position.x);
                }

                Debug.DrawLine(transform.position, targetPosition, Color.green, 0.5f); // デバッグ用
            }
        }

        // 移動処理
        if (moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                moving = false;
            }
        }
    }
}