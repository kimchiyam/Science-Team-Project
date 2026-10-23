using UnityEngine;

public class JunMenuWave : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Material lineMaterial;

    [Header("흰 곡선")]
    [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.65f);
    [SerializeField, Min(16)] private int pointCount = 128;
    [SerializeField] private float amplitude = 0.09f;
    [SerializeField] private float waveCount = 1.2f;
    [SerializeField] private float waveSpeed = 0.8f;
    [SerializeField] private float widthPixels = 2f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.18f;

    private LineRenderer line;
    private Vector3[] points;
    private float alpha = 1f;
    private bool fading;

    // 메뉴 전용 흰 곡선 준비
    private void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.sharedMaterial = lineMaterial;
        line.useWorldSpace = true;
        line.positionCount = pointCount;
        line.sortingOrder = -5;
        points = new Vector3[pointCount];
    }

    // 시작할 때 곡선 페이드
    public void FadeOut()
    {
        fading = true;
    }

    // 화면 가운데를 중심으로 곡선 출렁임
    private void LateUpdate()
    {
        if (fading) alpha = Mathf.MoveTowards(alpha, 0f, Time.unscaledDeltaTime / fadeDuration);
        if (alpha <= 0f)
        {
            line.enabled = false;
            enabled = false;
            return;
        }

        float phase = Time.unscaledTime * waveSpeed;
        float depth = Vector3.Dot(transform.position - gameCamera.transform.position, gameCamera.transform.forward);
        for (int i = 0; i < pointCount; i++)
        {
            float x = i / (float)(pointCount - 1);
            float y = 0.5f + Mathf.Sin(x * Mathf.PI * 2f * waveCount + phase) * amplitude;
            points[i] = gameCamera.ViewportToWorldPoint(new Vector3(x, y, depth));
        }
        Color color = lineColor;
        color.a *= alpha;
        line.startColor = line.endColor = color;
        line.startWidth = line.endWidth = gameCamera.orthographicSize * 2f / Mathf.Max(1, Screen.height) * widthPixels;
        line.SetPositions(points);
    }
}
