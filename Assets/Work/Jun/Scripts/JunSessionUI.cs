using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JunSessionUI : MonoBehaviour
{
    [Header("게임 연결")]
    [SerializeField] private PlayerController player;
    [SerializeField] private GameState gameState;
    [SerializeField] private PotentialTracker potentialTracker;

    [Header("시작 표시")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text countdownHint;
    [SerializeField] private CanvasGroup countdownGroup;
    [SerializeField, Min(0.1f)] private float readyDuration = 2f;
    [SerializeField] private Color startColor = new Color(0.45f, 0.94f, 1f);

    [Header("결과 표시")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private CanvasGroup resultsGroup;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text topSpeedText;
    [SerializeField] private TMP_Text maxDeltaText;
    [SerializeField] private TMP_Text perfectText;
    [SerializeField] private TMP_Text maxComboText;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject[] gameplayElements;
    [SerializeField, Min(0.01f)] private float voltsPerUnit = 10f;

    [Header("화면 전환")]
    [SerializeField, Min(0.01f)] private float popDuration = 0.22f;
    [SerializeField, Min(0f)] private float goHoldDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.25f;
    [SerializeField] private float initialScale = 0.85f;
    [SerializeField] private float goScale = 1.1f;

    private bool waitingForMenu;
    private float readyElapsed;
    private float maximumDeltaPotential;
    private int previousCount = -1;
    private bool started;
    private bool resultsShown;
    private Tween countdownTween;

    // 시작 화면과 재시작 버튼 준비
    private void Awake()
    {
        countdownGroup.alpha = waitingForMenu ? 0f : 1f;
        resultsPanel.SetActive(false);
        countdownHint.gameObject.SetActive(false);
        restartButton.onClick.AddListener(RestartRun);
    }

    // 게임 진행 상태에 맞춰 화면 전환
    private void LateUpdate()
    {
        if (waitingForMenu) return;
        if (gameState.Mode == PLayType.Ready)
        {
            UpdateCountdown();
            return;
        }

        if (gameState.Mode == PLayType.Play)
        {
            maximumDeltaPotential = Mathf.Max(maximumDeltaPotential, potentialTracker._deltaV * voltsPerUnit);
            if (!started) ShowGo();
            return;
        }

        if (!resultsShown) ShowResults();
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            RestartRun();
    }

    // 메인화면에서 시간과 게임 정보 표시 대기
    public void PrepareForMenu()
    {
        waitingForMenu = true;
        gameState.enabled = false;
        gameState.Mode = PLayType.Ready;
        countdownGroup.alpha = 0f;
        foreach (GameObject element in gameplayElements) element.SetActive(false);
    }

    // 시작 연출 뒤 기존 카운트다운 진행
    public void BeginRun()
    {
        waitingForMenu = false;
        readyElapsed = 0f;
        previousCount = -1;
        countdownGroup.alpha = 1f;
        foreach (GameObject element in gameplayElements) element.SetActive(true);
        gameState.Mode = PLayType.Ready;
        gameState.enabled = true;
    }
    // 기존 준비시간에 맞춘 카운트다운
    private void UpdateCountdown()
    {
        readyElapsed += Time.unscaledDeltaTime;
        int count = Mathf.Max(1, Mathf.CeilToInt(readyDuration - readyElapsed));
        if (count == previousCount) return;
        previousCount = count;
        countdownText.SetText("{0}", count);
        countdownText.color = Color.white;
        countdownTween?.Kill();
        countdownText.rectTransform.localScale = Vector3.one * initialScale;
        countdownTween = countdownText.rectTransform.DOScale(1f, popDuration).SetEase(Ease.OutBack).SetUpdate(true);
    }

    // 플레이 시작 표시
    private void ShowGo()
    {
        started = true;
        countdownTween?.Kill();
        countdownText.text = "Start!";
        countdownText.color = startColor;
        countdownHint.text = "";
        countdownText.rectTransform.localScale = Vector3.one * initialScale;
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(countdownText.rectTransform.DOScale(goScale, popDuration).SetEase(Ease.OutBack));
        sequence.AppendInterval(goHoldDuration);
        sequence.Append(countdownGroup.DOFade(0f, fadeDuration));
        countdownTween = sequence;
    }

    // 결과 표시와 기존 씬 번호 재시작 입력 정지
    private void ShowResults()
    {
        resultsShown = true;
        gameState.enabled = false;
        countdownTween?.Kill();
        countdownGroup.alpha = 0f;
        foreach (GameObject element in gameplayElements)
            element.SetActive(false);

        distanceText.SetText("{0:0} <size=60%>m</size>", Mathf.Max(0f, player.Distance));
        topSpeedText.SetText("{0:1} <size=60%>m/s</size>", player.MaxSpeedReached);
        maxDeltaText.SetText("{0:0} <size=60%>V</size>", maximumDeltaPotential);
        perfectText.SetText("{0:0}", player.PerfectCount);
        maxComboText.SetText("x{0:0}", player.MaxCombo);
        resultsPanel.SetActive(true);
        resultsGroup.alpha = 0f;
        resultsGroup.DOFade(1f, fadeDuration).SetUpdate(true);
    }

    // 현재 Main 씬으로 재시작
    public void RestartRun()
    {
        if (!resultsShown) return;
        restartButton.interactable = false;
        Scene scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
            scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(scene.buildIndex);
#endif
    }

    // 진행 중인 UI 애니메이션 정리
    private void OnDestroy()
    {
        countdownTween?.Kill();
        if (resultsGroup != null) resultsGroup.DOKill();
    }
}





