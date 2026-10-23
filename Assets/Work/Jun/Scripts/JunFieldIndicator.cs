using TMPro;
using UnityEngine;

public class JunFieldIndicator : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private TMP_Text fieldText;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private RectTransform shaft;
    [SerializeField] private RectTransform upperHead;
    [SerializeField] private RectTransform lowerHead;
    [SerializeField] private CanvasGroup group;

    [Header("E 막대")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, -1.15f, 0f);
    [SerializeField, Min(0.01f)] private float voltsPerUnit = 10f;
    [SerializeField, Min(1f)] private float fullStrengthField = 20f;
    [SerializeField] private float minimumLength = 12f;
    [SerializeField] private float maximumLength = 64f;
    [SerializeField] private Vector2 labelOffset = new Vector2(6f, -12f);
    [SerializeField, Range(0f, 1f)] private float minimumAlpha = 0.45f;
    [SerializeField, Range(0f, 1f)] private float maximumAlpha = 0.95f;
    [SerializeField, Min(0.01f)] private float responseTime = 0.12f;

    private float displayedStrength;

    // 화살촉을 숨기고 전기장 막대 준비
    private void Awake()
    {
        fieldText.text = "E";
        upperHead.gameObject.SetActive(false);
        lowerHead.gameObject.SetActive(false);
        shaft.pivot = new Vector2(0f, 0.5f);
        shaft.anchoredPosition = Vector2.zero;
    }

    // 공 아래에서 전기장 세기와 방향을 막대로 표시
    private void LateUpdate()
    {
        if (MapCreateManager.instance == null)
        {
            group.alpha = 0f;
            return;
        }

        Vector3 screenPosition = gameCamera.WorldToScreenPoint(player.transform.position + worldOffset);
        if (screenPosition.z <= 0f)
        {
            group.alpha = 0f;
            return;
        }

        transform.position = new Vector3(screenPosition.x, screenPosition.y, 0f);
        float slope = MapCreateManager.instance.GetSlope(player.transform.position.x);
        float strength = Mathf.Clamp01(Mathf.Abs(slope) * voltsPerUnit / fullStrengthField);
        displayedStrength = Mathf.MoveTowards(displayedStrength, strength, Time.deltaTime / responseTime);
        group.alpha = Mathf.Lerp(minimumAlpha, maximumAlpha, displayedStrength);
        arrow.gameObject.SetActive(!Mathf.Approximately(slope, 0f));

        Vector2 direction = new Vector2(-Mathf.Sign(slope), -Mathf.Abs(slope)).normalized;
        arrow.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        float length = Mathf.Lerp(minimumLength, maximumLength, displayedStrength);
        shaft.sizeDelta = new Vector2(length, shaft.sizeDelta.y);
        Vector2 textOffset = new Vector2(labelOffset.x * direction.x, labelOffset.y);
        fieldText.rectTransform.anchoredPosition = direction * length + textOffset;
    }
}
