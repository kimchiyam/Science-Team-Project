using DG.Tweening;
using TMPro;
using UnityEngine;

public class JunLandingFeedback : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private PlayerController player;
    [SerializeField] private RectTransform feedbackRoot;
    [SerializeField] private TMP_Text judgementText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private CanvasGroup group;

    [Header("에너지 전환 안내")]
    [SerializeField] private string energyMessage = "전위차 ΔV → 운동에너지 → 속도 증가";

    [Header("판정 색")]
    [SerializeField] private Color perfectColor = new Color(1f, 0.82f, 0.16f);
    [SerializeField] private Color goodColor = new Color(1f, 0.95f, 0.74f);
    [SerializeField] private Color badColor = new Color(1f, 0.28f, 0.22f);

    [Header("팝업과 페이드")]
    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private float settleDuration = 0.16f;
    [SerializeField] private float holdDuration = 0.35f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float riseDistance = 36f;
    [SerializeField] private float perfectScale = 1.16f;
    [SerializeField] private float goodScale = 1.08f;
    [SerializeField] private float badScale = 1.1f;

    [SerializeField] private float initialScale = 0.82f;

    private Vector2 startPosition;
    private Sequence popup;
    private Material glowMaterial;

    // 팝업 위치와 전용 발광 재질 준비
    private void Awake()
    {
        startPosition = feedbackRoot.anchoredPosition;
        glowMaterial = new Material(judgementText.fontSharedMaterial);
        judgementText.fontSharedMaterial = glowMaterial;
        judgementText.UpdateMeshPadding();
        group.alpha = 0f;
    }

    // 기존 착지 이벤트 연결
    private void OnEnable()
    {
        player.OnPerfect += ShowPerfect;
        player.OnGoodLanding += ShowGood;
        player.OnBadLanding += ShowBad;
    }

    // 착지 이벤트와 진행 중 연출 해제
    private void OnDisable()
    {
        player.OnPerfect -= ShowPerfect;
        player.OnGoodLanding -= ShowGood;
        player.OnBadLanding -= ShowBad;
        popup?.Kill();
        group.alpha = 0f;
    }

    // 완벽한 착지와 콤보 표시
    public void ShowPerfect(int combo)
    {
        ShowFeedback("완벽!", energyMessage + "\n<size=80%>콤보 x" + combo + "</size>", perfectColor, perfectScale);
    }

    // 부드러운 착지 표시
    public void ShowGood()
    {
        ShowFeedback("좋아요!", energyMessage, goodColor, goodScale);
    }

    // 착지 실패와 속도 감소 표시
    public void ShowBad()
    {
        ShowFeedback("아쉬워요!", "", badColor, badScale);
    }

    // 판정 글자 팝업과 이동 페이드
    private void ShowFeedback(string judgement, string detail, Color color, float scale)
    {
        popup?.Kill();
        feedbackRoot.anchoredPosition = startPosition;
        feedbackRoot.localScale = Vector3.one * initialScale;
        group.alpha = 1f;
        judgementText.text = judgement;
        judgementText.color = color;
        detailText.text = detail;
        detailText.color = new Color(color.r, color.g, color.b, 0.75f);
        glowMaterial.SetColor(ShaderUtilities.ID_GlowColor, new Color(color.r, color.g, color.b, 0.35f));

        popup = DOTween.Sequence();
        popup.Append(feedbackRoot.DOScale(scale, popDuration).SetEase(Ease.OutBack));
        popup.Append(feedbackRoot.DOScale(1f, settleDuration).SetEase(Ease.OutQuad));
        popup.AppendInterval(holdDuration);
        popup.Append(group.DOFade(0f, fadeDuration));
        popup.Join(feedbackRoot.DOAnchorPosY(startPosition.y + riseDistance, fadeDuration).SetEase(Ease.OutQuad));
    }

    // 생성한 발광 재질 정리
    private void OnDestroy()
    {
        popup?.Kill();
        if (glowMaterial != null) Destroy(glowMaterial);
    }
}



