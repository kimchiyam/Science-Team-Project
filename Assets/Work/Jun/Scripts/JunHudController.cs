using TMPro;
using UnityEngine;

public class JunHudController : MonoBehaviour
{
    [Header("게임 연결")]
    [SerializeField] private PlayerController player;
    [SerializeField] private GameState gameState;
    [SerializeField] private PotentialTracker potentialTracker;
    [SerializeField] private Camera gameCamera;

    [Header("HUD 텍스트")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text deltaPotentialText;
    [SerializeField] private TMP_Text electricFieldText;

    [Header("전위 눈금")]
    [SerializeField] private RectTransform potentialAxis;
    [SerializeField] private TMP_Text[] potentialLabels;
    [SerializeField, Min(0.01f)] private float voltsPerUnit = 10f;
    [SerializeField, Min(1f)] private float potentialStep = 50f;
    [SerializeField, Range(0f, 1f)] private float axisBottom = 0.12f;
    [SerializeField, Range(0f, 1f)] private float axisTop = 0.82f;

    // 현재 게임 정보 표시
    private void Update()
    {
        RefreshDisplay();
    }

    // HUD 값 갱신
    public void RefreshDisplay()
    {
        float remainingTime = Mathf.Max(0f, gameState.EndTime - gameState.CurrentTime);
        int seconds = Mathf.CeilToInt(remainingTime);
        timeText.text = $"{seconds / 60:00}:{seconds % 60:00}";
        distanceText.SetText("{0:0} <size=60%>m</size>", Mathf.Max(0f, player.Distance));
        speedText.SetText("{0:1} <size=50%>m/s</size>", player.Speed);

        float deltaPotential = potentialTracker._deltaV * voltsPerUnit;
        float electricField = 0f;
        if (MapCreateManager.instance != null)
        {
            electricField = Mathf.Abs(MapCreateManager.instance.GetSlope(player.transform.position.x)) * voltsPerUnit;
        }

        deltaPotentialText.SetText("{0:0} <size=50%>V</size>", deltaPotential);
        electricFieldText.SetText("{0:1} <size=50%>V/m</size>", electricField);
        RefreshPotentialAxis();
    }

    // 카메라 높이에 맞춰 전위 눈금 배치
    private void RefreshPotentialAxis()
    {
        float bottomY = gameCamera.ViewportToWorldPoint(new Vector3(0f, axisBottom, gameCamera.nearClipPlane)).y;
        float topY = gameCamera.ViewportToWorldPoint(new Vector3(0f, axisTop, gameCamera.nearClipPlane)).y;
        float firstPotential = Mathf.Ceil(bottomY * voltsPerUnit / potentialStep) * potentialStep;

        for (int i = 0; i < potentialLabels.Length; i++)
        {
            float potential = firstPotential + i * potentialStep;
            TMP_Text label = potentialLabels[i];
            bool visible = potential <= topY * voltsPerUnit;
            label.gameObject.SetActive(visible);
            if (!visible) continue;

            float viewportY = gameCamera.WorldToViewportPoint(new Vector3(0f, potential / voltsPerUnit, 0f)).y;
            label.rectTransform.anchoredPosition = new Vector2(0f, viewportY * potentialAxis.rect.height);
            label.SetText("{0:0} V", potential);
        }
    }
}
