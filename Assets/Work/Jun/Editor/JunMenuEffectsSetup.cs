using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class JunMenuEffectsSetup
{
    // Main 메뉴에 얼굴 후광과 움직이는 곡선 연결
    [MenuItem("Tools/Jun/Apply Menu Effects")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Work/Jun/Main.unity")
            throw new System.InvalidOperationException("재생을 중지하고 Jun/Main에서 실행해 주세요.");

        JunMainMenu menu = Object.FindFirstObjectByType<JunMainMenu>();
        SerializedObject settings = new SerializedObject(menu);
        Transform contents = ((CanvasGroup)settings.FindProperty("menuContents").objectReferenceValue).transform;
        RectTransform icon = (RectTransform)settings.FindProperty("icon").objectReferenceValue;
        GameObject goal = ((TMPro.TMP_Text)settings.FindProperty("goalText").objectReferenceValue).gameObject;
        Undo.RecordObject(goal, "Hide menu time description");
        goal.SetActive(false);

        Transform haloTransform = contents.Find("PlayerHalo");
        if (haloTransform == null)
        {
            GameObject haloObject = new GameObject("PlayerHalo", typeof(RectTransform), typeof(Image));
            haloObject.layer = 5;
            haloTransform = haloObject.transform;
            haloTransform.SetParent(contents, false);
            Undo.RegisterCreatedObjectUndo(haloObject, "Create player halo");
        }
        RectTransform halo = (RectTransform)haloTransform;
        halo.anchorMin = halo.anchorMax = halo.pivot = new Vector2(0.5f, 0.5f);
        halo.anchoredPosition = icon.anchoredPosition;
        halo.sizeDelta = new Vector2(420f, 420f);
        halo.SetAsFirstSibling();
        Image image = halo.GetComponent<Image>();
        image.sprite = CreateHaloSprite();
        image.color = new Color(1f, 1f, 1f, 0.9f);
        image.raycastTarget = false;

        Transform waveTransform = menu.transform.Find("MenuWave");
        if (waveTransform == null)
        {
            GameObject waveObject = new GameObject("MenuWave");
            waveTransform = waveObject.transform;
            waveTransform.SetParent(menu.transform, false);
            Undo.RegisterCreatedObjectUndo(waveObject, "Create menu wave");
        }
        JunMenuWave wave = waveTransform.GetComponent<JunMenuWave>();
        if (wave == null) wave = waveTransform.gameObject.AddComponent<JunMenuWave>();
        SerializedObject waveSettings = new SerializedObject(wave);
        waveSettings.FindProperty("gameCamera").objectReferenceValue = Camera.main;
        waveSettings.FindProperty("lineMaterial").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Work/Jun/Materials/WindLines.mat");
        waveSettings.ApplyModifiedProperties();
        settings.FindProperty("backgroundWave").objectReferenceValue = wave;
        settings.FindProperty("rotationSpeed").floatValue = 15f;
        settings.FindProperty("hoverRotationSpeed").floatValue = 100f;
        settings.FindProperty("rotationAcceleration").floatValue = 180f;
        settings.ApplyModifiedProperties();

        TMPro.TMP_Text title = contents.Find("Title").GetComponent<TMPro.TMP_Text>();
        JunRainbowTitle rainbow = title.GetComponent<JunRainbowTitle>();
        if (rainbow == null) rainbow = Undo.AddComponent<JunRainbowTitle>(title.gameObject);
        SerializedObject rainbowSettings = new SerializedObject(rainbow);
        Gradient gradient = new Gradient();
        gradient.SetKeys(new[]
        {
            new GradientColorKey(new Color(1f, 0.42f, 0.42f), 0f),
            new GradientColorKey(new Color(1f, 0.7f, 0.35f), 1f / 6f),
            new GradientColorKey(new Color(1f, 0.95f, 0.48f), 2f / 6f),
            new GradientColorKey(new Color(0.47f, 0.94f, 0.62f), 3f / 6f),
            new GradientColorKey(new Color(0.41f, 0.87f, 0.96f), 4f / 6f),
            new GradientColorKey(new Color(0.52f, 0.59f, 1f), 5f / 6f),
            new GradientColorKey(new Color(0.83f, 0.55f, 1f), 1f)
        }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        rainbowSettings.FindProperty("rainbow").gradientValue = gradient;
        rainbowSettings.ApplyModifiedProperties();
        title.color = Color.white;
        title.SetVerticesDirty();

        SerializedObject session = new SerializedObject(Object.FindFirstObjectByType<JunSessionUI>());
        TMPro.TMP_Text hint = (TMPro.TMP_Text)session.FindProperty("countdownHint").objectReferenceValue;
        hint.gameObject.SetActive(false);
        TMPro.TMP_Text count = (TMPro.TMP_Text)session.FindProperty("countdownText").objectReferenceValue;
        count.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        count.fontSharedMaterial = count.font.material;
        EditorUtility.SetDirty(count);
        EditorUtility.SetDirty(title);
        EditorUtility.SetDirty(image);
        EditorUtility.SetDirty(halo);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    // 길이가 다른 빛줄기가 퍼지는 후광 이미지 준비
    private static Sprite CreateHaloSprite()
    {
        const string folder = "Assets/Work/Jun/Textures";
        const string path = folder + "/MenuHaloRays.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Work/Jun", "Textures");

        const int size = 512;
        const int rayCount = 56;
        System.Random random = new System.Random(3719);
        Vector2[] directions = new Vector2[rayCount];
        float[] lengths = new float[rayCount];
        float[] widths = new float[rayCount];
        for (int i = 0; i < rayCount; i++)
        {
            float angle = (i + (float)random.NextDouble() * 0.7f) * Mathf.PI * 2f / rayCount;
            directions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            lengths[i] = Mathf.Lerp(0.52f, 0.98f, (float)random.NextDouble());
            widths[i] = i % 5 == 0 ? 0.025f : Mathf.Lerp(0.003f, 0.009f, (float)random.NextDouble());
        }

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                float radiusSquared = point.sqrMagnitude;
                float light = Mathf.Exp(-radiusSquared / 0.018f)
                    + Mathf.Exp(-radiusSquared / 0.12f) * 0.22f;
                float rayLight = 0f;
                for (int i = 0; i < rayCount; i++)
                {
                    float along = Vector2.Dot(point, directions[i]);
                    if (along <= 0f || along >= lengths[i]) continue;
                    float remaining = 1f - along / lengths[i];
                    float width = widths[i] * remaining + 0.0008f;
                    float across = Mathf.Abs(point.x * directions[i].y - point.y * directions[i].x);
                    if (across > width * 3f) continue;
                    float intensity = Mathf.Exp(-across * across / (width * width))
                        * Mathf.Sqrt(remaining) * 0.85f;
                    rayLight = Mathf.Max(rayLight, intensity);
                }
                Color color = Color.Lerp(new Color(1f, 0.97f, 0.8f),
                    new Color(1f, 0.72f, 0.28f), Mathf.Clamp01(Mathf.Sqrt(radiusSquared)));
                color.a = Mathf.Clamp01(light + rayLight);
                pixels[y * size + x] = color;
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        File.WriteAllBytes(Path.Combine(Application.dataPath, "Work/Jun/Textures/MenuHaloRays.png"), texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}

