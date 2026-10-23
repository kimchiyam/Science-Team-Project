using UnityEngine;

public class PlayerRoll : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField, Min(0.01f)] private float rollingRadius = 0.3f;
    [SerializeField] private float rotationMultiplier = 1f;

    private Vector2 previousPosition;
    private bool hasPreviousPosition;

    // 회전 기준 위치 초기화
    private void OnEnable()
    {
        hasPreviousPosition = false;
    }

    // 이동 거리에 맞춰 공 회전
    private void LateUpdate()
    {
        Vector2 currentPosition = player.position;
        if (!hasPreviousPosition)
        {
            previousPosition = currentPosition;
            hasPreviousPosition = true;
            return;
        }

        Vector2 movement = currentPosition - previousPosition;
        previousPosition = currentPosition;
        if (Mathf.Approximately(movement.x, 0f)) return;

        float angle = movement.magnitude / rollingRadius * Mathf.Rad2Deg;
        transform.Rotate(0f, 0f, -Mathf.Sign(movement.x) * angle * rotationMultiplier);
    }
}
