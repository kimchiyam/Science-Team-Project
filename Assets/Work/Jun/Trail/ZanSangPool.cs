using UnityEngine;

public class ZanSangPool : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private SpriteRenderer ghostPrefab;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameState gameState;

    [Header("잔상 길이와 간격")]
    [SerializeField, Min(1)] private int poolSize = 12;
    [SerializeField, Min(0.01f)] private float spawnInterval = 0.03f;
    [SerializeField, Min(0.01f)] private float slowSpawnInterval = 0.09f;
    [SerializeField, Min(0.01f)] private float lifeTime = 0.3f;
    [SerializeField, Min(1f)] private float fullTrailSpeed = 50f;
    [SerializeField, Min(0f)] private float minimumSpeed = 0.5f;

    [Header("잔상 색")]
    [SerializeField, Range(0f, 1f)] private float saturation = 0.5f;
    [SerializeField, Range(0f, 1f)] private float brightness = 0.85f;
    [SerializeField, Range(0f, 1f)] private float initialAlpha = 0.45f;
    [SerializeField, Range(0f, 1f)] private float hueStep = 0.035f;

    private SpriteRenderer[] ghosts;
    private float[] remainingLifetimes;
    private int currentIndex;
    private float spawnTimer;
    private float hue;

    // 재사용할 잔상 생성
    private void Awake()
    {
        ghosts = new SpriteRenderer[poolSize];
        remainingLifetimes = new float[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            ghosts[i] = Instantiate(ghostPrefab, transform);
            ghosts[i].sortingLayerID = playerRenderer.sortingLayerID;
            ghosts[i].sortingOrder = playerRenderer.sortingOrder - 1;
            ghosts[i].gameObject.SetActive(false);
        }
    }

    // 잔상 페이드와 속도별 생성 간격 갱신
    private void LateUpdate()
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!ghosts[i].gameObject.activeSelf) continue;
            remainingLifetimes[i] -= Time.deltaTime;
            if (remainingLifetimes[i] <= 0f)
            {
                ghosts[i].gameObject.SetActive(false);
                continue;
            }

            Color color = ghosts[i].color;
            color.a = initialAlpha * remainingLifetimes[i] / lifeTime;
            ghosts[i].color = color;
        }

        if (gameState.Mode != PLayType.Play || player.Speed < minimumSpeed)
        {
            spawnTimer = 0f;
            return;
        }

        float speedRatio = Mathf.Clamp01(player.Speed / fullTrailSpeed);
        float interval = Mathf.Lerp(slowSpawnInterval, spawnInterval, speedRatio);
        spawnTimer += Time.deltaTime;
        if (spawnTimer < interval) return;
        spawnTimer = 0f;
        SpawnGhost();
    }

    // 현재 공의 모습으로 잔상 배치
    private void SpawnGhost()
    {
        SpriteRenderer ghost = ghosts[currentIndex];
        ghost.transform.SetPositionAndRotation(playerRenderer.transform.position, playerRenderer.transform.rotation);
        Vector3 playerScale = playerRenderer.transform.lossyScale;
        Vector3 parentScale = transform.lossyScale;
        ghost.transform.localScale = new Vector3(
            playerScale.x / parentScale.x,
            playerScale.y / parentScale.y,
            playerScale.z / parentScale.z);
        ghost.sprite = playerRenderer.sprite;
        ghost.flipX = playerRenderer.flipX;
        ghost.flipY = playerRenderer.flipY;

        hue = Mathf.Repeat(hue + hueStep, 1f);
        Color color = Color.HSVToRGB(hue, saturation, brightness);
        color.a = initialAlpha;
        ghost.color = color;
        remainingLifetimes[currentIndex] = lifeTime;
        ghost.gameObject.SetActive(true);
        currentIndex = (currentIndex + 1) % poolSize;
    }
}
