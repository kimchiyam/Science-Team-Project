using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 전위 서핑 플레이어
// - Space(또는 마우스/터치) 누르기: 전기장에 강하게 붙음 → 내리막 가속, 공중에서 빨리 내려옴
// - 떼기: 약하게 붙음 → 언덕 꼭대기에서 날아감
// - 착지 각도가 내리막 경사와 맞으면 PERFECT
//
// Rigidbody2D를 쓰지 않고, MapCreateManager의 curve(높이)와 GetSlope(기울기)로 직접 움직임.
public class PlayerController : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private GameState gameState;

    [Header("조작감 - 중력(전기장 세기)")]
    [Tooltip("Space를 뗐을 때. 낮을수록 멀리 날아감")]
    [SerializeField] private float lightGravity = 23f;
    [Tooltip("Space를 누를 때. 높을수록 내리막 가속이 세고 빨리 착지")]
    [SerializeField] private float heavyGravity = 78f;

    [Header("조작감 - 속도")]
    [SerializeField] private float startSpeed = 8.4f;
    [SerializeField] private float maxSpeed = 68f;
    [Tooltip("뗐을 때 최저 속도. 골짜기에 갇히지 않게 해 줌")]
    [SerializeField] private float minSpeed = 5f;
    [Tooltip("누르고 있을 때 최저 속도. 오르막에서 누르면 손해 보게")]
    [SerializeField] private float minSpeedPressing = 1.6f;
    [Tooltip("땅에서 초당 줄어드는 속도 비율")]
    [SerializeField] private float friction = 0.035f;

    [Header("착지 판정")]
    [Tooltip("이 각도(라디안) 안이면 PERFECT. 0.24 ≈ 14도")]
    [SerializeField] private float perfectAngle = 0.24f;
    [Tooltip("이 각도(라디안)보다 크면 BAD. 0.62 ≈ 35도")]
    [SerializeField] private float badAngle = 0.62f;
    [Tooltip("이 시간보다 짧게 뜬 건 착지로 안 침 (작은 튐 무시)")]
    [SerializeField] private float minAirTime = 0.18f;
    [SerializeField] private float perfectBoost = 1.06f;
    [Tooltip("콤보 1당 추가 보너스 (최대 8콤보까지)")]
    [SerializeField] private float comboBoost = 0.012f;
    [SerializeField] private float badPenalty = 0.72f;

    [Header("보이는 모습")]
    [Tooltip("공 반지름. 공이 선 위에 올라앉게 위치를 올려 줌")]
    [SerializeField] private float radius = 0.3f;

    // ── 다른 스크립트가 읽어 갈 값 ──
    public float Speed => _velocity.magnitude;
    public float MaxSpeedReached { get; private set; }
    public float Distance => _pos.x - _startX;
    public bool IsGrounded { get; private set; } = true;
    public bool IsPressing { get; private set; }
    public int PerfectCount { get; private set; }
    public int Combo { get; private set; }
    public int MaxCombo { get; private set; }

    // ── 연출 친구가 붙일 이벤트 ──
    public event Action<int> OnPerfect;   // 인자: 현재 콤보
    public event Action OnGoodLanding;
    public event Action OnBadLanding;

    private const float Step = 1f / 240f; // 물리를 잘게 쪼개 계산 (빠를 때 땅 뚫림 방지)
    private const float DownhillMin = -0.12f;
    private const float LiftOffGap = 0.03f;

    private Vector2 _pos;      // 공의 "바닥" 위치 (땅에 닿는 점)
    private Vector2 _velocity;
    private float _airTime;
    private float _accumulator;
    private float _startX;
    private float _endX;
    private bool _initialized;

    private AnimationCurve Curve => MapCreateManager.instance.curve;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (MapCreateManager.instance == null || Curve.length < 2) return;

        _startX = Curve.keys[0].time + 0.5f;
        _endX = Curve.keys[Curve.length - 1].time;

        _pos = new Vector2(_startX, Curve.Evaluate(_startX));
        _velocity = Tangent(_startX) * startSpeed;
        IsGrounded = true;
        _initialized = true;
        ApplyTransform();
    }

    private void Update()
    {
        if (!_initialized)
        {
            Init(); // 맵이 늦게 만들어진 경우 대비
            return;
        }

        IsPressing = ReadInput();

        // 플레이 중일 때만 움직임
        if (gameState != null && gameState.Mode != PLayType.Play) return;

        // 프레임 시간을 작은 조각으로 나눠 계산
        _accumulator += Mathf.Min(Time.deltaTime, 0.05f); // 렉 걸린 프레임에 순간이동 방지
        while (_accumulator >= Step)
        {
            PhysicsStep(Step);
            _accumulator -= Step;
        }

        ApplyTransform();
    }

    private bool ReadInput()
    {
        bool key = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        bool mouse = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool touch = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        return key || mouse || touch;
    }

    private void PhysicsStep(float dt)
    {
        // 1. 전기장(중력처럼 아래로 당김). 누르면 강하게
        float g = IsPressing ? heavyGravity : lightGravity;
        _velocity.y -= g * dt;
        _pos += _velocity * dt;

        // 맵 끝에서 멈춤
        if (_pos.x >= _endX)
        {
            _pos.x = _endX;
            _velocity = Vector2.zero;
        }

        float groundY = Curve.Evaluate(_pos.x);
        float slope = MapCreateManager.instance.GetSlope(_pos.x);

        if (_pos.y <= groundY)
        {
            // 2. 땅에 닿음: 속도를 경사 방향으로 맞춤
            Vector2 tangent = Tangent(_pos.x);
            float mult = 1f;
            if (!IsGrounded) mult = Land(slope);

            float speed = Vector2.Dot(_velocity, tangent) * mult;
            speed *= 1f - friction * dt;
            float floor = IsPressing ? minSpeedPressing : minSpeed;
            speed = Mathf.Clamp(speed, floor, maxSpeed);

            _velocity = tangent * speed;
            _pos.y = groundY;
            IsGrounded = true;
            _airTime = 0f;
        }
        else if (_pos.y > groundY + LiftOffGap)
        {
            // 3. 공중
            if (IsGrounded)
            {
                IsGrounded = false;
                _airTime = 0f;
            }
            _airTime += dt;
        }

        MaxSpeedReached = Mathf.Max(MaxSpeedReached, Speed);
    }

    // 착지 판정. 속도에 곱할 배율을 돌려줌
    private float Land(float slope)
    {
        if (_airTime < minAirTime) return 1f; // 작은 튐은 무시

        float flyAngle = Mathf.Atan2(_velocity.y, _velocity.x);
        float groundAngle = Mathf.Atan(slope);
        float diff = Mathf.Abs(flyAngle - groundAngle);

        if (slope < DownhillMin && diff < perfectAngle)
        {
            Combo++;
            PerfectCount++;
            MaxCombo = Mathf.Max(MaxCombo, Combo);
            OnPerfect?.Invoke(Combo);
            return perfectBoost + Mathf.Min(Combo, 8) * comboBoost;
        }

        Combo = 0;
        if (diff > badAngle)
        {
            OnBadLanding?.Invoke();
            return badPenalty;
        }

        OnGoodLanding?.Invoke();
        return 1f;
    }

    // x 위치의 경사 방향 (길이 1인 벡터)
    private Vector2 Tangent(float x)
    {
        float slope = MapCreateManager.instance.GetSlope(x);
        return new Vector2(1f, slope).normalized;
    }

    private void ApplyTransform()
    {
        // 공이 선에 파묻히지 않게 반지름만큼 위로
        transform.position = new Vector3(_pos.x, _pos.y + radius, transform.position.z);
    }
}