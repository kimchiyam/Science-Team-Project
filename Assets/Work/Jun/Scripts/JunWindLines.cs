using UnityEngine;

public class JunWindLines : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private PlayerController player;
    [SerializeField] private GameState gameState;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private LineRenderer[] lines;

    [Header("속도별 변화")]
    [SerializeField] private float startSpeed = 12f;
    [SerializeField] private float fullEffectSpeed = 55f;
    [SerializeField, Min(0)] private int maximumVisibleLines = 30;
    [SerializeField, Min(0.01f)] private float responseSpeed = 2f;
    [SerializeField] private float minimumMoveSpeed = 0.16f;
    [SerializeField] private float maximumMoveSpeed = 0.75f;
    [SerializeField] private float minimumLength = 0.025f;
    [SerializeField] private float maximumLength = 0.085f;

    [Header("선 모양")]
    [SerializeField] private Color lineColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField, Range(0f, 1f)] private float maximumAlpha = 0.16f;
    [SerializeField, Min(0.1f)] private float widthPixels = 1.4f;
    [SerializeField, Range(0f, 1f)] private float bottomMargin = 0.06f;
    [SerializeField, Range(0f, 1f)] private float topMargin = 0.9f;

    private Vector2[] positions;
    private float[] lengthScales;
    private float strength;

    // 재사용할 선의 시작 위치 준비
    private void Awake()
    {
        positions = new Vector2[lines.Length];
        lengthScales = new float[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            positions[i] = new Vector2(Random.value, Random.Range(bottomMargin, topMargin));
            lengthScales[i] = Random.Range(0.7f, 1.3f);
        }
    }

    // 속도에 맞춰 바람선 이동과 농도 갱신
    private void LateUpdate()
    {
        float targetStrength = gameState.Mode == PLayType.Play
            ? Mathf.InverseLerp(startSpeed, fullEffectSpeed, player.Speed)
            : 0f;
        strength = Mathf.MoveTowards(strength, targetStrength, responseSpeed * Time.deltaTime);
        int visibleCount = Mathf.RoundToInt(Mathf.Min(maximumVisibleLines, lines.Length) * strength);
        float moveSpeed = Mathf.Lerp(minimumMoveSpeed, maximumMoveSpeed, strength);
        float length = Mathf.Lerp(minimumLength, maximumLength, strength);
        float depth = Vector3.Dot(transform.position - gameCamera.transform.position, gameCamera.transform.forward);
        float width = gameCamera.orthographicSize * 2f / Screen.height * widthPixels;

        for (int i = 0; i < lines.Length; i++)
        {
            LineRenderer line = lines[i];
            line.enabled = i < visibleCount;
            if (!line.enabled) continue;

            float lineLength = length * lengthScales[i];
            positions[i].x -= moveSpeed * Time.deltaTime;
            if (positions[i].x < -lineLength)
            {
                positions[i].x = 1f + lineLength;
                positions[i].y = Random.Range(bottomMargin, topMargin);
            }

            float edgeFade = Mathf.Clamp01(Mathf.Min(positions[i].x, 1f - positions[i].x) / 0.12f);
            Color color = lineColor;
            color.a = maximumAlpha * strength * edgeFade;
            line.startColor = color;
            line.endColor = color;
            line.startWidth = line.endWidth = width;
            line.SetPosition(0, gameCamera.ViewportToWorldPoint(new Vector3(positions[i].x, positions[i].y, depth)));
            line.SetPosition(1, gameCamera.ViewportToWorldPoint(new Vector3(positions[i].x + lineLength, positions[i].y, depth)));
        }
    }
}
