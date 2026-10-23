using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public static class JunMainMenuSetup
{
    private const string MainPath = "Assets/Work/Jun/Main.unity";
    private static readonly Color White = new Color(0.94f, 0.97f, 1f);
    private static readonly Color Muted = new Color(0.94f, 0.97f, 1f, 0.58f);
    private static readonly Color Cyan = new Color(0.35f, 0.9f, 0.95f);

    // Main 씬에 메인화면과 공통 디자인 연결
    [MenuItem("Tools/Jun/Build Main Menu")]
    public static void Build()
    {
        if (Application.isPlaying || EditorSceneManager.GetActiveScene().path != MainPath)
            throw new System.InvalidOperationException("재생을 중지하고 Jun/Main에서 실행해 주세요.");
        if (Object.FindFirstObjectByType<JunMainMenu>() != null)
            throw new System.InvalidOperationException("메인화면이 이미 있습니다.");

        GameObject hud = GameObject.Find("JunHUD");
        TMP_FontAsset font = CreateMenuFont();
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        SpriteRenderer visual = player.GetComponentInChildren<SpriteRenderer>();
        Camera camera = Camera.main;

        RectTransform root = CreateRect("MainMenu", hud.transform, Vector2.zero, Vector2.zero);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;

        RectTransform contents = CreateRect("Contents", root, Vector2.zero, Vector2.zero);
        contents.anchorMin = Vector2.zero;
        contents.anchorMax = Vector2.one;
        contents.offsetMin = contents.offsetMax = Vector2.zero;
        CanvasGroup contentGroup = contents.gameObject.AddComponent<CanvasGroup>();
        contentGroup.blocksRaycasts = false;
        contentGroup.interactable = false;

        TMP_Text eyebrow = CreateText("Eyebrow", contents, font, "전위 서핑",
            16f, Cyan, new Vector2(0f, 440f), new Vector2(900f, 30f));
        eyebrow.characterSpacing = 8f;
        TMP_Text title = CreateText("Title", contents, font, "전위 서핑",
            112f, White, new Vector2(0f, 340f), new Vector2(1300f, 160f));
        title.fontStyle = FontStyles.Bold;
        CreateText("Subject", contents, font, "과학 팀 프로젝트 · 전위와 전기장",
            28f, White, new Vector2(0f, 225f), new Vector2(1300f, 50f));
        CreateText("Description", contents, font, "전위에너지가 운동에너지로 바뀌는 순간",
            22f, Muted, new Vector2(0f, 178f), new Vector2(1300f, 42f));
        TMP_Text hint = CreateText("ClickHint", contents, font, "← 클릭하여 시작",
            29f, White, new Vector2(270f, 15f), new Vector2(380f, 64f));

        RectTransform divider = CreateRect("GuideDivider", contents, new Vector2(0f, -350f), new Vector2(1200f, 1f));
        Image dividerImage = divider.gameObject.AddComponent<Image>();
        dividerImage.color = new Color(1f, 1f, 1f, 0.13f);
        dividerImage.raycastTarget = false;
        CreateText("GuideTitle", contents, font, "스페이스 또는 마우스 클릭으로 조작",
            19f, Muted, new Vector2(0f, -386f), new Vector2(1200f, 36f));
        CreateText("GuidePress", contents, font, "내리막에서 누르기",
            23f, White, new Vector2(-430f, -435f), new Vector2(400f, 42f));
        CreateText("GuideRelease", contents, font, "오르막에서 떼기",
            23f, White, new Vector2(0f, -435f), new Vector2(400f, 42f));
        CreateText("GuideLanding", contents, font, "내리막에 맞춰 착지",
            23f, White, new Vector2(430f, -435f), new Vector2(400f, 42f));
        TMP_Text goal = CreateText("Goal", contents, font, "최대한 멀리 이동하세요",
            18f, Muted, new Vector2(0f, -495f), new Vector2(1200f, 36f));

        RectTransform icon = CreateRect("PlayerIcon", root, new Vector2(0f, 15f), new Vector2(170f, 170f));
        Image iconImage = icon.gameObject.AddComponent<Image>();
        iconImage.sprite = visual.sprite;
        iconImage.preserveAspect = true;
        Button button = icon.gameObject.AddComponent<Button>();
        button.targetGraphic = iconImage;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.8f, 1f, 1f);
        colors.pressedColor = new Color(0.65f, 0.9f, 0.95f);
        button.colors = colors;
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        GameObject presentation = new GameObject("JunPresentation");
        JunMainMenu menu = presentation.AddComponent<JunMainMenu>();
        SerializedObject menuSettings = new SerializedObject(menu);
        SetReference(menuSettings, "session", Object.FindFirstObjectByType<JunSessionUI>());
        SetReference(menuSettings, "goalText", goal);
        SetReference(menuSettings, "playerVisual", visual);
        SetReference(menuSettings, "gameCamera", camera);
        SetReference(menuSettings, "cameraBrain", camera.GetComponent<CinemachineBrain>());
        SetReference(menuSettings, "menuRoot", root.gameObject);
        SetReference(menuSettings, "menuContents", contentGroup);
        SetReference(menuSettings, "icon", icon);
        SetReference(menuSettings, "startButton", button);
        SetReference(menuSettings, "startHint", hint);
        SetReference(menuSettings, "canvasRect", (RectTransform)hud.transform);
        menuSettings.ApplyModifiedProperties();

        MapCreateManager map = Object.FindFirstObjectByType<MapCreateManager>();
        SerializedObject mapSettings = new SerializedObject(map);
        LineRenderer ground = (LineRenderer)mapSettings.FindProperty("lineRenderer").objectReferenceValue;
        mapSettings.FindProperty("arrowDistance").floatValue = 2.5f;
        mapSettings.ApplyModifiedProperties();

        Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Work/Jun/Materials/WindLines.mat");
        JunSceneStyle style = presentation.AddComponent<JunSceneStyle>();
        SerializedObject styleSettings = new SerializedObject(style);
        SetReference(styleSettings, "gameCamera", camera);
        SetReference(styleSettings, "groundLine", ground);
        SetReference(styleSettings, "fieldArrowPrefab", mapSettings.FindProperty("arrow").objectReferenceValue);
        SetReference(styleSettings, "lineMaterial", material);
        styleSettings.ApplyModifiedProperties();
        Undo.RecordObject(camera, "Set Main background");
        camera.backgroundColor = styleSettings.FindProperty("backgroundColor").colorValue;
        Undo.RecordObject(ground, "Set Main ground style");
        ground.sharedMaterial = material;
        ground.startColor = ground.endColor = styleSettings.FindProperty("groundColor").colorValue;
        ground.widthMultiplier = 1f;
        ground.startWidth = ground.endWidth = styleSettings.FindProperty("groundWidth").floatValue;

        Undo.RegisterCreatedObjectUndo(root.gameObject, "Create Jun main menu");
        Undo.RegisterCreatedObjectUndo(presentation, "Create Jun presentation");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Restyle();
        JunMenuEffectsSetup.Apply();
        Selection.activeGameObject = root.gameObject;
    }

    // 지정한 색상과 편집 가능한 기본 메뉴 적용
    [MenuItem("Tools/Jun/Apply Basic Main Style")]
    public static void Restyle()
    {
        if (Application.isPlaying || EditorSceneManager.GetActiveScene().path != MainPath)
            throw new System.InvalidOperationException("재생을 중지하고 Jun/Main에서 실행해 주세요.");

        Transform root = GameObject.Find("JunHUD").transform.Find("MainMenu");
        Transform contents = root.Find("Contents");
        foreach (string name in new[] { "PosterTitle", "SubjectPaper", "MenuWave", "IconShadow" })
        {
            Transform old = contents.Find(name);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        }
        foreach (string name in new[] { "Eyebrow", "Description", "GuideDivider", "GuideLanding" })
            contents.Find(name).gameObject.SetActive(false);
        contents.Find("Title").gameObject.SetActive(true);
        AdjustText(contents, "Title", "전위 서핑", 112f, Color.white,
            new Vector2(0f, 340f), new Vector2(1300f, 160f), 0f);
        AdjustText(contents, "Subject", "과학 팀 프로젝트 · 전위와 전기장", 28f, Color.white,
            new Vector2(0f, 225f), new Vector2(1300f, 50f), 0f);
        AdjustText(contents, "ClickHint", "← 클릭하여 시작", 29f, Color.white,
            new Vector2(270f, 15f), new Vector2(380f, 64f), 0f);
        AdjustText(contents, "GuideTitle", "스페이스 / 마우스 클릭", 20f, Color.white,
            new Vector2(0f, -385f), new Vector2(1200f, 40f), 0f);
        AdjustText(contents, "GuidePress", "내리막: 누르기", 23f, Color.white,
            new Vector2(-260f, -440f), new Vector2(460f, 42f), 0f);
        AdjustText(contents, "GuideRelease", "오르막: 떼기", 23f, Color.white,
            new Vector2(260f, -440f), new Vector2(460f, 42f), 0f);
        AdjustText(contents, "Goal", "최대한 멀리 이동하세요", 20f, Color.white,
            new Vector2(0f, -495f), new Vector2(1200f, 42f), 0f);
        RectTransform icon = (RectTransform)root.Find("PlayerIcon");
        Undo.RecordObject(icon, "Reset menu icon");
        icon.anchoredPosition = new Vector2(0f, 15f);
        icon.sizeDelta = new Vector2(170f, 170f);
        icon.localRotation = Quaternion.identity;

        JunMainMenu menu = Object.FindFirstObjectByType<JunMainMenu>();
        SerializedObject menuSettings = new SerializedObject(menu);
        menuSettings.ApplyModifiedProperties();
        JunSceneStyle style = Object.FindFirstObjectByType<JunSceneStyle>();
        SetReference(menuSettings, "sceneStyle", style);
        menuSettings.ApplyModifiedProperties();
        SerializedObject styleSettings = new SerializedObject(style);
        styleSettings.FindProperty("backgroundColor").colorValue = new Color(0.018f, 0.018f, 0.035f);
        styleSettings.FindProperty("groundColor").colorValue = Color.white;
        styleSettings.FindProperty("downhillColor").colorValue = Color.red;
        styleSettings.FindProperty("uphillColor").colorValue = Color.blue;
        styleSettings.FindProperty("arrowScale").floatValue = 1f;
        styleSettings.ApplyModifiedProperties();
        Camera.main.backgroundColor = styleSettings.FindProperty("backgroundColor").colorValue;
        LineRenderer ground = (LineRenderer)styleSettings.FindProperty("groundLine").objectReferenceValue;
        ground.startColor = ground.endColor = Color.white;

        SerializedObject map = new SerializedObject(Object.FindFirstObjectByType<MapCreateManager>());
        map.FindProperty("minimumPointDistance").intValue = 16;
        map.FindProperty("maximumPointDistance").intValue = 25;
        map.ApplyModifiedProperties();

        JunFieldIndicator field = Object.FindFirstObjectByType<JunFieldIndicator>();
        SerializedObject fieldSettings = new SerializedObject(field);
        fieldSettings.FindProperty("minimumLength").floatValue = 12f;
        fieldSettings.FindProperty("maximumLength").floatValue = 64f;
        fieldSettings.FindProperty("minimumAlpha").floatValue = 0.45f;
        fieldSettings.FindProperty("maximumAlpha").floatValue = 0.95f;
        fieldSettings.FindProperty("labelOffset").vector2Value = new Vector2(6f, -12f);
        fieldSettings.ApplyModifiedProperties();
        Color fieldColor = new Color(0.45f, 0.95f, 1f);
        foreach (Image image in field.GetComponentsInChildren<Image>(true)) image.color = fieldColor;
        TMP_Text label = field.GetComponentInChildren<TMP_Text>(true);
        label.color = fieldColor;
        label.text = "E";
        RectTransform bar = (RectTransform)fieldSettings.FindProperty("shaft").objectReferenceValue;
        bar.pivot = new Vector2(0f, 0.5f);
        bar.anchoredPosition = Vector2.zero;
        bar.sizeDelta = new Vector2(32f, 2.5f);
        ((RectTransform)fieldSettings.FindProperty("upperHead").objectReferenceValue).gameObject.SetActive(false);
        ((RectTransform)fieldSettings.FindProperty("lowerHead").objectReferenceValue).gameObject.SetActive(false);
        SerializedObject wind = new SerializedObject(Object.FindFirstObjectByType<SpeedLines>());
        wind.FindProperty("color").colorValue = new Color(0.86f, 0.94f, 1f);
        wind.ApplyModifiedProperties();

        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            text.font.TryAddCharacters(text.text, out string missing);
        EditorUtility.SetDirty(Camera.main);
        EditorUtility.SetDirty(ground);
        EditorUtility.SetDirty(label);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
    }

    // 기존 메뉴 글자의 위치와 모양 조정
    private static void AdjustText(Transform parent, string name, string value, float size,
        Color color, Vector2 position, Vector2 dimensions, float angle)
    {
        TMP_Text text = parent.Find(name).GetComponent<TMP_Text>();
        Undo.RecordObject(text, "Restyle menu text");
        Undo.RecordObject(text.rectTransform, "Place menu text");
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.characterSpacing = 0f;
        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = dimensions;
        text.rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
    }
    // 한국어 메뉴 글꼴과 아틀라스 준비
    private static TMP_FontAsset CreateMenuFont()
    {
        string path = "Assets/Work/Jun/Fonts/JunMenuFont.asset";
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (font != null) return font;
        Font source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Work/Jun/Fonts/NotoSansKR-Regular.otf");
        font = TMP_FontAsset.CreateFontAsset(source, 64, 8, GlyphRenderMode.SDFAA, 1024, 1024,
            AtlasPopulationMode.Dynamic);
        font.name = "JunMenuFont";
        font.atlasTextures[0].name = "JunMenuFont Atlas";
        font.material.name = "JunMenuFont Material";
        AssetDatabase.CreateAsset(font, path);
        AssetDatabase.AddObjectToAsset(font.material, font);
        AssetDatabase.AddObjectToAsset(font.atlasTextures[0], font);
        string characters = "전위 서핑과학팀프로젝트·와기장에너지가운동로바뀌는순간←클릭하여시작또마우스조내리막누르오떼맞춰착초동안최대한멀세요0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ";
        if (!font.TryAddCharacters(characters, out string missing))
            Debug.LogWarning("메뉴 글꼴 누락: " + missing);
        EditorUtility.SetDirty(font);
        return font;
    }

    // 중앙 기준 UI 영역 생성
    private static RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        item.layer = 5;
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    // 메뉴 글자 생성
    private static TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font,
        string value, float size, Color color, Vector2 position, Vector2 dimensions)
    {
        RectTransform rect = CreateRect(name, parent, position, dimensions);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    // Inspector 참조 연결
    private static void SetReference(SerializedObject settings, string property, Object value)
    {
        settings.FindProperty(property).objectReferenceValue = value;
    }
}







