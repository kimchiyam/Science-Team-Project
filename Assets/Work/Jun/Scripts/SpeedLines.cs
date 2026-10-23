using System.Collections.Generic;
using UnityEngine;

public class SpeedLines : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private PlayerController player;
    [SerializeField] private GameState gameState;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Material lineMaterial;

    [Header("속도와 잔여 시간")]
    [SerializeField] private float startSpeed = 22f;
    [SerializeField] private float fullSpeed = 50f;
    [SerializeField, Min(0.01f)] private float responseTime = 0.65f;
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.08f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.4f;

    [Header("모양")]
    [SerializeField, Min(1)] private int poolSize = 40;
    [SerializeField] private float spawnPerSecond = 70f;
    [SerializeField] private float minLength = 80f;
    [SerializeField] private float maxLength = 280f;
    [SerializeField] private float thickness = 3f;
    [SerializeField] private float baseMoveSpeed = 1600f;
    [SerializeField] private float moveSpeedPerUnit = 30f;
    [SerializeField, Min(1f)] private float referenceHeight = 1080f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.5f;
    [SerializeField] private Color color = new Color(0.86f, 0.94f, 1f);

    private class Line
    {
        public LineRenderer renderer;
        public float speed;
        public float lifetime;
        public float remainingTime;
        public float alpha;
    }

    private readonly List<Line> lines = new List<Line>();
    private float spawnAccumulator;
    private float strength;

    // 재사용할 월드 바람선 준비
    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject lineObject = new GameObject("SpeedLine");
            lineObject.transform.SetParent(transform, false);
            LineRenderer renderer = lineObject.AddComponent<LineRenderer>();
            renderer.sharedMaterial = lineMaterial;
            renderer.useWorldSpace = true;
            renderer.positionCount = 2;
            renderer.sortingOrder = 5;
            renderer.enabled = false;
            lines.Add(new Line { renderer = renderer });
        }
    }

    // 속도에 맞춰 생성량을 줄이고 남은 선 이동
    private void Update()
    {
        if (player == null || gameState == null || gameCamera == null) return;

        float deltaTime = Time.deltaTime;
        float targetStrength = gameState.Mode == PLayType.Play
            ? Mathf.InverseLerp(startSpeed, fullSpeed, player.Speed)
            : 0f;
        strength = Mathf.MoveTowards(strength, targetStrength, deltaTime / responseTime);
        spawnAccumulator += spawnPerSecond * strength * deltaTime;
        while (spawnAccumulator >= 1f)
        {
            Spawn();
            spawnAccumulator -= 1f;
        }

        foreach (Line line in lines)
        {
            if (!line.renderer.enabled) continue;

            line.remainingTime -= deltaTime;
            if (line.remainingTime <= 0f)
            {
                line.renderer.enabled = false;
                continue;
            }

            Vector3 movement = Vector3.left * line.speed * deltaTime;
            line.renderer.SetPosition(0, line.renderer.GetPosition(0) + movement);
            line.renderer.SetPosition(1, line.renderer.GetPosition(1) + movement);

            float fadeIn = Mathf.Clamp01((line.lifetime - line.remainingTime) / fadeInDuration);
            float fadeOut = Mathf.Clamp01(line.remainingTime / fadeOutDuration);
            Color lineColor = color;
            lineColor.a = line.alpha * Mathf.Min(fadeIn, fadeOut);
            line.renderer.startColor = line.renderer.endColor = lineColor;
        }
    }

    // 생성 순간의 카메라 위치에서 월드 바람선 배치
    private void Spawn()
    {
        Line line = null;
        foreach (Line candidate in lines)
        {
            if (candidate.renderer.enabled) continue;
            line = candidate;
            break;
        }
        if (line == null) return;

        float worldPerPixel = gameCamera.orthographicSize * 2f / referenceHeight;
        float length = Mathf.Lerp(minLength, maxLength, strength * Random.value) * worldPerPixel;
        float depth = Vector3.Dot(transform.position - gameCamera.transform.position, gameCamera.transform.forward);
        Vector3 position = gameCamera.ViewportToWorldPoint(new Vector3(1f, Random.value, depth));

        line.speed = (baseMoveSpeed + player.Speed * moveSpeedPerUnit) * worldPerPixel;
        float viewWidth = gameCamera.orthographicSize * 2f * gameCamera.aspect;
        line.lifetime = (viewWidth + length) / line.speed;
        line.remainingTime = line.lifetime;
        line.alpha = Mathf.Lerp(0.15f, maxAlpha, strength);
        line.renderer.startWidth = line.renderer.endWidth = thickness * worldPerPixel;
        line.renderer.SetPosition(0, position);
        line.renderer.SetPosition(1, position + Vector3.right * length);
        line.renderer.startColor = line.renderer.endColor = new Color(color.r, color.g, color.b, 0f);
        line.renderer.enabled = true;
    }
}
