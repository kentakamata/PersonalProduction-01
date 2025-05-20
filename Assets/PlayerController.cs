using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ShadowMask2D shadowMask; // ���͈͂��Q��
    public float moveSpeed = 5f;

    private Vector3 targetPosition;
    private bool moving = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        targetPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>(); // �X�v���C�g������ꍇ�̎擾
    }

    void Update()
    {
        // �ړ����łȂ��Ƃ������N���b�N��t
        if (!moving && Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            // ���E�������肵�Ĉړ�����
            if (shadowMask != null && shadowMask.IsVisible(mouseWorld))
            {
                targetPosition = mouseWorld;
                moving = true;

                // �N���b�N�����ɃX�v���C�g�𔽓]�i���E�j
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (mouseWorld.x < transform.position.x);
                }

                Debug.DrawLine(transform.position, targetPosition, Color.green, 0.5f); // �f�o�b�O�p
            }
        }

        // �ړ�����
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