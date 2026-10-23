using UnityEngine;

public class JunLandingBurst : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Material lineMaterial;

    [Header("착지 링과 빛 조각")]
    [SerializeField, Min(0.01f)] private float duration = 0.32f;
    [SerializeField, Min(0.01f)] private float perfectRadius = 1.25f;
    [SerializeField, Min(0.01f)] private float goodRadius = 0.8f;
    [SerializeField, Min(0.01f)] private float badRadius = 0.95f;
    [SerializeField, Min(0.01f)] private float lineWidth = 0.035f;
    [SerializeField, Range(0f, 1f)] private float maximumAlpha = 0.55f;
    [SerializeField, Range(0f, 1f)] private float ringHeight = 0.25f;
    [SerializeField, Min(8)] private int ringSegments = 32;
    [SerializeField, Min(2)] private int sparkCount = 6;
    [SerializeField] private Color perfectColor = new Color(1f, 0.82f, 0.16f);
    [SerializeField] private Color goodColor = new Color(1f, 0.95f, 0.74f);
    [SerializeField] private Color badColor = new Color(1f, 0.28f, 0.22f);

    private LineRenderer ring;
    private LineRenderer[] sparks;
    private Vector3 landingPosition;
    private Quaternion landingRotation;
    private Color burstColor;
    private float radius;
    private float elapsed;
    private bool active;

    // 재사용할 링과 빛 조각 준비
    private void Awake()
    {
        ring = CreateLine("LandingRing", ringSegments);
        ring.loop = true;
        sparks = new LineRenderer[sparkCount];
        for (int i = 0; i < sparks.Length; i++)
            sparks[i] = CreateLine("LandingSpark", 2);
    }

    // 착지 이벤트 연결
    private void OnEnable()
    {
        player.OnPerfect += ShowPerfect;
        player.OnGoodLanding += ShowGood;
        player.OnBadLanding += ShowBad;
    }

    // 착지 이벤트와 남은 효과 정리
    private void OnDisable()
    {
        player.OnPerfect -= ShowPerfect;
        player.OnGoodLanding -= ShowGood;
        player.OnBadLanding -= ShowBad;
        HideBurst();
    }

    // 월드 좌표를 사용하는 선 준비
    private LineRenderer CreateLine(string objectName, int pointCount)
    {
        GameObject item = new GameObject(objectName);
        item.transform.SetParent(transform, false);
        LineRenderer line = item.AddComponent<LineRenderer>();
        line.sharedMaterial = lineMaterial;
        line.useWorldSpace = true;
        line.positionCount = pointCount;
        line.startWidth = line.endWidth = lineWidth;
        line.sortingOrder = 6;
        line.enabled = false;
        return line;
    }

    // 정확한 착지 효과
    private void ShowPerfect(int combo)
    {
        ShowBurst(perfectColor, perfectRadius);
    }

    // 부드러운 착지 효과
    private void ShowGood()
    {
        ShowBurst(goodColor, goodRadius);
    }

    // 거친 착지 효과
    private void ShowBad()
    {
        ShowBurst(badColor, badRadius);
    }

    // 착지 지점과 바닥 기울기 저장
    private void ShowBurst(Color color, float effectRadius)
    {
        MapCreateManager map = MapCreateManager.instance;
        float x = player.transform.position.x;
        landingPosition = new Vector3(x, map.curve.Evaluate(x) + 0.04f, 0f);
        landingRotation = Quaternion.Euler(0f, 0f, Mathf.Atan(map.GetSlope(x)) * Mathf.Rad2Deg);
        burstColor = color;
        radius = effectRadius;
        elapsed = 0f;
        active = true;
    }

    // 착지 링을 넓히고 빛 조각 페이드
    private void Update()
    {
        if (!active) return;
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        if (progress >= 1f)
        {
            HideBurst();
            return;
        }

        float spread = Mathf.Lerp(0.15f, radius, 1f - (1f - progress) * (1f - progress));
        Color color = burstColor;
        color.a = maximumAlpha * (1f - progress) * (1f - progress);
        ring.enabled = true;
        ring.startColor = ring.endColor = color;
        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / ring.positionCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * spread, Mathf.Sin(angle) * spread * ringHeight, 0f);
            ring.SetPosition(i, landingPosition + landingRotation * offset);
        }

        for (int i = 0; i < sparks.Length; i++)
        {
            float angle = Mathf.Lerp(20f, 160f, i / (float)(sparks.Length - 1)) * Mathf.Deg2Rad;
            Vector3 direction = landingRotation * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            Vector3 position = landingPosition + direction * spread;
            sparks[i].enabled = true;
            sparks[i].startColor = sparks[i].endColor = color;
            sparks[i].SetPosition(0, position);
            sparks[i].SetPosition(1, position + direction * (radius * 0.18f * (1f - progress)));
        }
    }

    // 남은 링과 빛 조각 숨김
    private void HideBurst()
    {
        active = false;
        if (ring == null) return;
        ring.enabled = false;
        foreach (LineRenderer spark in sparks) spark.enabled = false;
    }
}
