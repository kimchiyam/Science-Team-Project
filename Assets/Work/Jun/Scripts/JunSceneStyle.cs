using UnityEngine;

[DefaultExecutionOrder(100)]
public class JunSceneStyle : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private LineRenderer groundLine;
    [SerializeField] private GameObject fieldArrowPrefab;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private JunTerrainFill groundFill;

    [Header("지형과 배경")]
    [SerializeField] private Color backgroundColor = new Color(0.018f, 0.018f, 0.035f);
    [SerializeField] private Color groundColor = Color.white;
    [SerializeField] private float groundWidth = 0.075f;
    [SerializeField] private Color downhillColor = Color.red;
    [SerializeField] private Color uphillColor = Color.blue;
    [SerializeField] private float arrowScale = 0.85f;

    private Transform fieldArrows;
    private bool terrainVisible = true;

    // 생성된 지형과 방향 화살표 스타일 적용
    private void Start()
    {
        gameCamera.backgroundColor = backgroundColor;
        groundLine.sharedMaterial = lineMaterial;
        groundLine.startColor = groundLine.endColor = groundColor;
        groundLine.widthMultiplier = 1f;
        groundLine.startWidth = groundLine.endWidth = groundWidth;
        groundLine.enabled = terrainVisible;

        fieldArrows = new GameObject("FieldArrows").transform;
        fieldArrows.SetParent(transform, false);
        string cloneName = fieldArrowPrefab.name + "(Clone)";
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            if (root.name != cloneName) continue;
            root.transform.SetParent(fieldArrows, true);
            root.transform.localScale *= arrowScale;
            foreach (SpriteRenderer sprite in root.GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.color = sprite.color.r > sprite.color.b ? downhillColor : uphillColor;
                sprite.sharedMaterial = lineMaterial;
            }
        }
        fieldArrows.gameObject.SetActive(terrainVisible);
    }

    // 메뉴와 플레이에 맞춰 실제 지형 표시
    public void SetTerrainVisible(bool visible)
    {
        terrainVisible = visible;
        if (groundFill != null) groundFill.SetVisible(visible);
        groundLine.enabled = visible;
        if (fieldArrows != null) fieldArrows.gameObject.SetActive(visible);
    }
}


