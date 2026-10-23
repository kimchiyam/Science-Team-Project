using UnityEngine;

[DefaultExecutionOrder(110)]
public class JunTerrainFill : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private MapCreateManager map;
    [SerializeField] private LineRenderer groundLine;
    [SerializeField] private Material baseMaterial;

    [Header("높이에 따른 바닥 색")]
    [SerializeField] private Gradient groundGradient = new Gradient();
    [SerializeField] private float minimumHeight = 0f;
    [SerializeField] private float maximumHeight = 10f;
    [SerializeField, Range(32, 512)] private int sampleCount = 192;
    [SerializeField, Min(0f)] private float screenMargin = 2f;

    [Header("테두리 빛")]
    [SerializeField] private Color glowColor = new Color(1f, 0.82f, 0.5f, 0.16f);
    [SerializeField] private float glowWidth = 0.18f;

    private Mesh mesh;
    private MeshRenderer fillRenderer;
    private Material fillMaterial;
    private Texture2D gradientTexture;
    private LineRenderer glow;
    private Vector3[] vertices;
    private Vector2[] textureCoordinates;
    private Vector3[] edgePoints;
    private bool visible = true;

    // 그라데이션 텍스처와 재사용할 지형 메시 준비
    private void Start()
    {
        gradientTexture = new Texture2D(1, 256, TextureFormat.RGBA32, false);
        gradientTexture.name = "TerrainHeightGradient";
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
        gradientTexture.filterMode = FilterMode.Bilinear;
        Color[] colors = new Color[gradientTexture.height];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = groundGradient.Evaluate(i / (float)(colors.Length - 1));
        gradientTexture.SetPixels(colors);
        gradientTexture.Apply();

        fillMaterial = new Material(baseMaterial);
        fillMaterial.mainTexture = gradientTexture;
        GameObject fill = new GameObject("TerrainGradient", typeof(MeshFilter), typeof(MeshRenderer));
        fill.transform.SetParent(transform, false);
        fillRenderer = fill.GetComponent<MeshRenderer>();
        fillRenderer.sharedMaterial = fillMaterial;
        fillRenderer.sortingLayerID = groundLine.sortingLayerID;
        fillRenderer.sortingOrder = groundLine.sortingOrder - 2;
        fillRenderer.enabled = visible;
        mesh = new Mesh { name = "VisibleTerrainFill" };
        mesh.MarkDynamic();
        fill.GetComponent<MeshFilter>().sharedMesh = mesh;

        vertices = new Vector3[sampleCount * 2];
        textureCoordinates = new Vector2[vertices.Length];
        edgePoints = new Vector3[sampleCount];
        int[] triangles = new int[(sampleCount - 1) * 6];
        for (int i = 0; i < sampleCount - 1; i++)
        {
            int vertex = i * 2;
            int triangle = i * 6;
            triangles[triangle] = vertex;
            triangles[triangle + 1] = vertex + 2;
            triangles[triangle + 2] = vertex + 1;
            triangles[triangle + 3] = vertex + 1;
            triangles[triangle + 4] = vertex + 2;
            triangles[triangle + 5] = vertex + 3;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        GameObject edge = new GameObject("TerrainEdgeGlow", typeof(LineRenderer));
        edge.transform.SetParent(transform, false);
        glow = edge.GetComponent<LineRenderer>();
        glow.sharedMaterial = baseMaterial;
        glow.useWorldSpace = true;
        glow.positionCount = sampleCount;
        glow.startColor = glow.endColor = glowColor;
        glow.startWidth = glow.endWidth = glowWidth;
        glow.sortingLayerID = groundLine.sortingLayerID;
        glow.sortingOrder = groundLine.sortingOrder - 1;
        glow.enabled = visible;
    }

    // 카메라에 보이는 바닥을 연속된 높이 색으로 채우기
    private void LateUpdate()
    {
        if (!visible || mesh == null || map.curve.length < 2) return;
        Vector3 cameraPosition = gameCamera.transform.position;
        float halfWidth = gameCamera.orthographicSize * gameCamera.aspect;
        float left = Mathf.Max(cameraPosition.x - halfWidth - screenMargin, map.curve[0].time);
        float right = Mathf.Min(cameraPosition.x + halfWidth + screenMargin,
            map.curve[map.curve.length - 1].time);
        float bottom = cameraPosition.y - gameCamera.orthographicSize - screenMargin;
        float heightRange = Mathf.Max(0.01f, maximumHeight - minimumHeight);

        for (int i = 0; i < sampleCount; i++)
        {
            float x = Mathf.Lerp(left, right, i / (float)(sampleCount - 1));
            float y = map.curve.Evaluate(x);
            Vector3 topPoint = new Vector3(x, y, 0f);
            vertices[i * 2] = transform.InverseTransformPoint(topPoint);
            vertices[i * 2 + 1] = transform.InverseTransformPoint(new Vector3(x, bottom, 0f));
            textureCoordinates[i * 2] = new Vector2(0.5f, (y - minimumHeight) / heightRange);
            textureCoordinates[i * 2 + 1] = new Vector2(0.5f, (bottom - minimumHeight) / heightRange);
            edgePoints[i] = topPoint;
        }
        mesh.vertices = vertices;
        mesh.uv = textureCoordinates;
        mesh.RecalculateBounds();
        glow.SetPositions(edgePoints);
    }

    // 메뉴와 플레이에 맞춰 바닥 표시
    public void SetVisible(bool show)
    {
        visible = show;
        if (fillRenderer != null) fillRenderer.enabled = show;
        if (glow != null) glow.enabled = show;
    }

    // 실행 중 생성한 그래픽 자원 정리
    private void OnDestroy()
    {
        if (mesh != null) Destroy(mesh);
        if (fillMaterial != null) Destroy(fillMaterial);
        if (gradientTexture != null) Destroy(gradientTexture);
    }
}

