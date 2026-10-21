using UnityEngine;

public class ZanSangPool : MonoBehaviour
{
    [SerializeField] private SpriteRenderer ghostPrefab;
    [SerializeField] private SpriteRenderer playerRenderer;
    
    [SerializeField] private int poolSize = 12;
    [SerializeField] private float spawnInterval = 0.03f;
    [SerializeField] private float lifeTime = 0.3f;
    
    private Transform[] ghostTransforms;
    private SpriteRenderer[] ghostRenderers;
    private float[] lifeTimes;

    private int currentIndex;
    private float spawnTimer;
    private float hue;
    
    private void Awake()
    {
        ghostTransforms = new Transform[poolSize];
        ghostRenderers = new SpriteRenderer[poolSize];
        lifeTimes = new float[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            SpriteRenderer ghost = Instantiate(ghostPrefab, transform);

            ghostTransforms[i] = ghost.transform;
            ghostRenderers[i] = ghost;

            ghost.gameObject.SetActive(false);
        }
    }
    private void SpawnGhost()
    {
        int i = currentIndex;

        ghostTransforms[i].position = playerRenderer.transform.position;
        ghostTransforms[i].rotation = playerRenderer.transform.rotation;
        ghostTransforms[i].localScale = playerRenderer.transform.localScale;
        ghostRenderers[i].sprite = playerRenderer.sprite; 
        
        hue += 0.05f;
        if (hue > 1f) hue = 0f;
        Color color = Color.HSVToRGB(hue, 0.6f, 1f);
        color.a = 0.8f;
        ghostRenderers[i].color = color;

        ghostRenderers[i].gameObject.SetActive(true);

        lifeTimes[i] = lifeTime;

        currentIndex = (currentIndex + 1) % poolSize;
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnGhost();
            spawnTimer = 0;
        }
        
        for (int i = 0; i < poolSize; i++)
        {
            if (lifeTimes[i] <= 0f)
            {
                ghostRenderers[i].gameObject.SetActive(false);
                continue;
            }
            
            float alpha = lifeTimes[i] / lifeTime;
            Color color = ghostRenderers[i].color;
            color.a = alpha;
            ghostRenderers[i].color = color;
            
            lifeTimes[i] -= Time.deltaTime;
        }
    }
}

