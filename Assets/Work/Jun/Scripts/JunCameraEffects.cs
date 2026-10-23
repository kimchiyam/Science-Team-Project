using Unity.Cinemachine;
using UnityEngine;

public class JunCameraEffects : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private PlayerController player;
    [SerializeField] private GameState gameState;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineCameraOffset cameraOffset;

    [Header("속도별 줌")]
    [SerializeField] private float zoomStartSpeed = 10f;
    [SerializeField] private float fullZoomSpeed = 55f;
    [SerializeField, Min(0f)] private float maximumZoomOut = 2f;
    [SerializeField, Min(0.01f)] private float zoomSmoothTime = 0.55f;

    [Header("착지 흔들림")]
    [SerializeField, Min(0f)] private float perfectStrength = 0.1f;
    [SerializeField, Min(0f)] private float goodStrength = 0.045f;
    [SerializeField, Min(0f)] private float badStrength = 0.22f;
    [SerializeField, Min(0.01f)] private float perfectDuration = 0.18f;
    [SerializeField, Min(0.01f)] private float goodDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float badDuration = 0.24f;
    [SerializeField, Min(1f)] private float shakeFrequency = 24f;

    private float baseSize;
    private Vector3 baseOffset;
    private float zoomVelocity;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeStrength;

    // 기존 카메라 구도 보관
    private void Awake()
    {
        baseSize = virtualCamera.Lens.OrthographicSize;
        baseOffset = cameraOffset.Offset;
    }

    // 착지 판정 이벤트 연결
    private void OnEnable()
    {
        player.OnPerfect += ShakePerfect;
        player.OnGoodLanding += ShakeGood;
        player.OnBadLanding += ShakeBad;
    }

    // 이벤트 해제와 기존 구도 복원
    private void OnDisable()
    {
        player.OnPerfect -= ShakePerfect;
        player.OnGoodLanding -= ShakeGood;
        player.OnBadLanding -= ShakeBad;
        if (!Application.isPlaying) return;
        virtualCamera.Lens.OrthographicSize = baseSize;
        cameraOffset.Offset = baseOffset;
        shakeTimer = 0f;
    }

    // 속도에 따른 부드러운 줌과 착지 흔들림 갱신
    private void Update()
    {
        float speedRatio = gameState.Mode == PLayType.Play
            ? Mathf.InverseLerp(zoomStartSpeed, fullZoomSpeed, player.Speed)
            : 0f;
        float targetSize = baseSize + maximumZoomOut * speedRatio;
        virtualCamera.Lens.OrthographicSize = Mathf.SmoothDamp(
            virtualCamera.Lens.OrthographicSize, targetSize, ref zoomVelocity, zoomSmoothTime);

        if (shakeTimer <= 0f)
        {
            cameraOffset.Offset = baseOffset;
            return;
        }

        shakeTimer = Mathf.Max(0f, shakeTimer - Time.deltaTime);
        float fade = shakeTimer / shakeDuration;
        float amplitude = shakeStrength * fade * fade;
        float sample = Time.time * shakeFrequency;
        float x = Mathf.PerlinNoise(sample, 0f) * 2f - 1f;
        float y = Mathf.PerlinNoise(0f, sample + 1f) * 2f - 1f;
        cameraOffset.Offset = baseOffset + new Vector3(x, y, 0f) * amplitude;
    }

    // PERFECT 착지 흔들림
    private void ShakePerfect(int combo)
    {
        StartShake(perfectStrength, perfectDuration);
    }

    // GOOD 착지 흔들림
    private void ShakeGood()
    {
        StartShake(goodStrength, goodDuration);
    }

    // BAD 착지 흔들림
    private void ShakeBad()
    {
        StartShake(badStrength, badDuration);
    }

    // 강한 흔들림을 우선해 착지 효과 시작
    private void StartShake(float strength, float duration)
    {
        if (shakeTimer > 0f && strength < shakeStrength) return;
        shakeStrength = strength;
        shakeDuration = duration;
        shakeTimer = duration;
    }
}
