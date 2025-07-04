using UnityEngine;

public class ItemVision : MonoBehaviour
{
    [Header("共有される視界の範囲")]
    public float shareRadius = 3f;

    [Header("プレイヤーがこの範囲に入ると視界共有される範囲")]
    public float visionRadius = 3f;
}
