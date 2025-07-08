using UnityEngine;

public class ItemVision : MonoBehaviour
{
    [Header("共有される視界の範囲")]
    public float visionRadius = 5f;

    [Header("プレイヤーがこの範囲に入ると視界共有される")]
    public float shareRadius = 3f;
}
