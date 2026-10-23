using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class JunMainMenu : MonoBehaviour
{
    [Header("게임 연결")]
    [SerializeField] private JunSessionUI session;
    [SerializeField] private JunSceneStyle sceneStyle;
    [SerializeField] private SpriteRenderer playerVisual;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private CinemachineBrain cameraBrain;

    [Header("메뉴 연결")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private CanvasGroup menuContents;
    [SerializeField] private RectTransform icon;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text startHint;
    [SerializeField] private TMP_Text goalText;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private JunMenuWave backgroundWave;

    [Header("시작 연출")]
    [SerializeField, Min(0.1f)] private float dropDuration = 0.9f;
    [SerializeField] private float menuCameraLift = 4.5f;
    [SerializeField] private float iconRotation = -180f;
    [SerializeField] private float impactScale = 1.12f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float hoverRotationSpeed = 100f;
    [SerializeField] private float rotationAcceleration = 180f;

    private Vector3 playCameraPosition;
    private Vector3 menuCameraPosition;
    private Vector2 iconStartPosition;
    private Vector2 iconStartSize;
    private Quaternion iconStartRotation;
    private float elapsed;
    private bool ready;
    private bool dropping;
    private bool finished;
    private float currentRotationSpeed;

    // 메인화면에서 게임 진행 대기
    private void Awake()
    {
        session.PrepareForMenu();
        sceneStyle.SetTerrainVisible(false);
        currentRotationSpeed = rotationSpeed;
        goalText.gameObject.SetActive(false);
        playerVisual.enabled = false;
        startButton.onClick.AddListener(StartDrop);
        startHint.alpha = 1f;
    }

    // 기존 카메라 구도 보관과 메뉴 구도 적용
    private IEnumerator Start()
    {
        yield return null;
        playCameraPosition = gameCamera.transform.position;
        menuCameraPosition = playCameraPosition + Vector3.up * menuCameraLift;
        cameraBrain.enabled = false;
        gameCamera.transform.position = menuCameraPosition;
        ready = true;
    }

    // 아이콘 클릭으로 낙하 시작
    private void StartDrop()
    {
        if (!ready || dropping || finished) return;
        dropping = true;
        elapsed = 0f;
        iconStartPosition = icon.anchoredPosition;
        iconStartSize = icon.sizeDelta;
        iconStartRotation = icon.localRotation;
        startButton.interactable = false;
        sceneStyle.SetTerrainVisible(true);
        menuContents.DOFade(0f, 0.25f);
        backgroundWave.FadeOut();
    }

    // 아이콘 낙하와 카메라 복귀
    private void LateUpdate()
    {
        if (!dropping)
        {
            if (ready && !finished) RotateIcon();
            return;
        }
        elapsed += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(elapsed / dropDuration);
        float fall = progress * progress;
        float cameraProgress = progress * progress * (3f - 2f * progress);
        gameCamera.transform.position = Vector3.Lerp(menuCameraPosition, playCameraPosition, cameraProgress);

        Vector3 screenPosition = gameCamera.WorldToScreenPoint(playerVisual.bounds.center);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out Vector2 target);
        icon.anchoredPosition = Vector2.Lerp(iconStartPosition, target, fall);
        float worldDiameter = playerVisual.bounds.size.y;
        float iconHeight = worldDiameter / (gameCamera.orthographicSize * 2f) * canvasRect.rect.height;
        Vector2 targetSize = new Vector2(iconHeight * iconStartSize.x / iconStartSize.y, iconHeight);
        icon.sizeDelta = Vector2.Lerp(iconStartSize, targetSize, progress);
        icon.localRotation = iconStartRotation * Quaternion.Euler(0f, 0f, iconRotation * progress);
        if (progress >= 1f) FinishDrop();
    }

    // 마우스를 올리면 얼굴 회전 속도 증가
    private void RotateIcon()
    {
        bool hovered = Mouse.current != null && RectTransformUtility.RectangleContainsScreenPoint(
            icon, Mouse.current.position.ReadValue(), null);
        float targetSpeed = hovered ? hoverRotationSpeed : rotationSpeed;
        currentRotationSpeed = Mathf.MoveTowards(currentRotationSpeed, targetSpeed,
            rotationAcceleration * Time.unscaledDeltaTime);
        icon.Rotate(0f, 0f, -currentRotationSpeed * Time.unscaledDeltaTime);
    }
    // 실제 플레이어로 전환하고 카운트다운 시작
    private void FinishDrop()
    {
        dropping = false;
        finished = true;
        playerVisual.transform.rotation = Quaternion.Euler(0f, 0f, icon.localEulerAngles.z);
        playerVisual.enabled = true;
        cameraBrain.enabled = true;
        playerVisual.transform.DOPunchScale(Vector3.one * (impactScale - 1f), 0.18f, 1, 0.3f);
        menuRoot.SetActive(false);
        session.BeginRun();
    }

    // 메뉴 애니메이션 정리
    private void OnDestroy()
    {
        if (menuContents != null) menuContents.DOKill();
        if (playerVisual != null) playerVisual.transform.DOKill();
    }
}



