using UnityEngine;

/// <summary>맵의 Collider2D에 붙여 구간별 경사 감각을 조절합니다.</summary>
public sealed class PotentialSurface2D : MonoBehaviour
{
    [Min(0f)] public float gravityMultiplier = 1f;
    [Min(0f)] public float rollingResistanceMultiplier = 1f;
    [Min(0f)] public float maximumSpeedMultiplier = 1f;
}