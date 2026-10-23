using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class JunHudSetup
{
    private const string MainScenePath = "Assets/Work/Jun/Main.unity";
    private static readonly Color ValueColor = new Color(0.94f, 0.96f, 1f, 0.95f);
    private static readonly Color LabelColor = new Color(0.77f, 0.83f, 0.9f, 0.58f);

    // Main 씬에 HUD 생성과 연결
    [MenuItem("Tools/Jun/Build Main HUD")]
    public static void Build()
    {
        if (Application.isPlaying || EditorSceneManager.GetActiveScene().path != MainScenePath)
        {
            Debug.LogError("재생을 중지하고 Jun/Main 씬에서 실행해 주세요.");
            return;
        }

        if (GameObject.Find("JunHUD") != null)
        {
            Debug.Log("JunHUD가 이미 있습니다. Hierarchy에서 수정해 주세요.");
            return;
        }

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        GameState gameState = Object.FindFirstObjectByType<GameState>();
        PotentialTracker tracker = Object.FindFirstObjectByType<PotentialTracker>();
        Camera gameCamera = Camera.main;
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        if (player == null || gameState == null || tracker == null || gameCamera == null || font == null)
        {
            Debug.LogError("플레이어, GameState, PotentialTracker, Main Camera, TMP 폰트를 확인해 주세요.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Build Jun HUD");

        RectTransform root = CreateRect("JunHUD", null, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
        Canvas canvas = root.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = root.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform top = CreateRect("TopHUD", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -32f), new Vector2(440f, 175f));
        CreateText("TimeLabel", top, font, "남은 시간", 13f, LabelColor, new Vector2(0f, 0f), new Vector2(440f, 22f), true);
        TMP_Text time = CreateText("TimeValue", top, font, "00:40", 56f, ValueColor,
            new Vector2(0f, -24f), new Vector2(440f, 68f), true);
        CreateText("DistanceLabel", top, font, "이동 거리", 11f, LabelColor,
            new Vector2(0f, -99f), new Vector2(440f, 18f), true);
        TMP_Text distance = CreateText("DistanceValue", top, font, "0 m", 28f, ValueColor,
            new Vector2(0f, -120f), new Vector2(440f, 40f), true);

        RectTransform right = CreateRect("RightHUD", root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-42f, 0f), new Vector2(220f, 370f));
        CreateText("DataLabel", right, font, "주행 정보", 12f, LabelColor, Vector2.zero, new Vector2(220f, 22f)).gameObject.SetActive(false);
        CreateLine("Divider", right, new Vector2(0f, -32f), new Vector2(150f, 1f));
        CreateText("SpeedLabel", right, font, "속도", 13f, LabelColor, new Vector2(0f, -55f), new Vector2(220f, 24f));
        TMP_Text speed = CreateText("SpeedValue", right, font, "0.0 m/s", 40f, ValueColor,
            new Vector2(0f, -82f), new Vector2(220f, 58f));
        CreateText("DeltaVLabel", right, font, "\u0394V", 16f, LabelColor, new Vector2(0f, -164f), new Vector2(220f, 24f));
        TMP_Text deltaV = CreateText("DeltaVValue", right, font, "0 V", 40f, ValueColor,
            new Vector2(0f, -192f), new Vector2(220f, 58f));
        CreateText("FieldLabel", right, font, "E", 16f, LabelColor, new Vector2(0f, -274f), new Vector2(220f, 24f));
        TMP_Text field = CreateText("FieldValue", right, font, "0.0 V/m", 40f, ValueColor,
            new Vector2(0f, -302f), new Vector2(220f, 58f));

        RectTransform axis = CreateRect("PotentialAxis", root, Vector2.zero, Vector2.zero,
            new Vector2(36f, 0f), new Vector2(130f, 0f));
        axis.anchorMax = new Vector2(0f, 1f);
        axis.offsetMin = new Vector2(36f, 0f);
        axis.offsetMax = new Vector2(166f, 0f);
        RectTransform axisTitle = CreateRect("PotentialTitle", root, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(36f, -150f), new Vector2(180f, 24f));
        CreateText("Label", axisTitle, font, "전위 / V", 12f, LabelColor, Vector2.zero, new Vector2(180f, 24f));

        TMP_Text[] potentialLabels = new TMP_Text[10];
        for (int i = 0; i < potentialLabels.Length; i++)
        {
            TMP_Text label = CreateText("PotentialTick" + i, axis, font, "", 18f,
                new Color(0.8f, 0.86f, 0.93f, 0.48f), Vector2.zero, new Vector2(80f, 28f));
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.zero;
            label.rectTransform.pivot = new Vector2(0f, 0.5f);
            RectTransform tick = CreateLine("Tick", label.rectTransform, Vector2.zero, new Vector2(24f, 1f));
            tick.anchorMin = new Vector2(1f, 0.5f);
            tick.anchorMax = new Vector2(1f, 0.5f);
            tick.pivot = new Vector2(0f, 0.5f);
            tick.anchoredPosition = new Vector2(8f, 0f);
            potentialLabels[i] = label;
        }

        JunHudController controller = root.gameObject.AddComponent<JunHudController>();
        SerializedObject settings = new SerializedObject(controller);
        SetReference(settings, "player", player);
        SetReference(settings, "gameState", gameState);
        SetReference(settings, "potentialTracker", tracker);
        SetReference(settings, "gameCamera", gameCamera);
        SetReference(settings, "timeText", time);
        SetReference(settings, "distanceText", distance);
        SetReference(settings, "speedText", speed);
        SetReference(settings, "deltaPotentialText", deltaV);
        SetReference(settings, "electricFieldText", field);
        SetReference(settings, "potentialAxis", axis);
        SerializedProperty labels = settings.FindProperty("potentialLabels");
        labels.arraySize = potentialLabels.Length;
        for (int i = 0; i < potentialLabels.Length; i++)
            labels.GetArrayElementAtIndex(i).objectReferenceValue = potentialLabels[i];
        settings.ApplyModifiedPropertiesWithoutUndo();

        foreach (LeftTimeUI oldTime in Object.FindObjectsByType<LeftTimeUI>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(oldTime.gameObject, "Hide old time display");
            oldTime.gameObject.SetActive(false);
        }

        foreach (HudDisplay oldHud in Object.FindObjectsByType<HudDisplay>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(oldHud, "Disable old HUD");
            oldHud.enabled = false;
        }

        Canvas.ForceUpdateCanvases();
        controller.RefreshDisplay();
        Undo.RegisterCreatedObjectUndo(root.gameObject, "Build Jun HUD");
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = root.gameObject;
        Debug.Log("Jun Main HUD 생성 완료: 시간, 거리, 속도, 전위차, 전기장, 전위 눈금 연결.");
    }

    // Main 씬에 잔상과 전기장 및 착지 피드백 연결
    [MenuItem("Tools/Jun/Build Player Feedback")]
    public static void BuildPlayerFeedback()
    {
        if (Application.isPlaying || EditorSceneManager.GetActiveScene().path != MainScenePath)
        {
            Debug.LogError("재생을 중지하고 Jun/Main 씬에서 실행해 주세요.");
            return;
        }

        GameObject hud = GameObject.Find("JunHUD");
        if (hud == null || hud.transform.Find("LandingFeedback") != null)
        {
            Debug.LogError("JunHUD가 없거나 착지 피드백이 이미 있습니다.");
            return;
        }

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        GameState gameState = Object.FindFirstObjectByType<GameState>();
        SpriteRenderer visual = player.GetComponentInChildren<SpriteRenderer>();
        TMP_FontAsset font = hud.GetComponentInChildren<TMP_Text>(true).font;
        ZanSangPool ghosts = Object.FindFirstObjectByType<ZanSangPool>();
        Undo.RecordObject(ghosts, "Connect speed trail");
        SerializedObject ghostSettings = new SerializedObject(ghosts);
        SetReference(ghostSettings, "player", player);
        SetReference(ghostSettings, "gameState", gameState);
        SetReference(ghostSettings, "playerRenderer", visual);
        ghostSettings.ApplyModifiedProperties();

        RectTransform fieldRoot = CreateRect("FieldIndicator", hud.transform, new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180f, 80f));
        fieldRoot.gameObject.SetActive(false);
        CanvasGroup fieldGroup = fieldRoot.gameObject.AddComponent<CanvasGroup>();
        fieldGroup.blocksRaycasts = false;
        fieldGroup.interactable = false;
        TMP_Text fieldLabel = CreateText("FieldValue", fieldRoot, font, "E 0.0", 22f,
            new Color(0.72f, 0.87f, 1f), Vector2.zero, new Vector2(180f, 28f), true);
        RectTransform arrow = CreateRect("Arrow", fieldRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -5f), Vector2.zero);
        RectTransform shaft = CreateLine("Shaft", arrow, new Vector2(-16f, 0f), new Vector2(32f, 2.5f));
        shaft.anchorMin = shaft.anchorMax = new Vector2(0.5f, 0.5f);
        shaft.pivot = new Vector2(0f, 0.5f);
        RectTransform upper = CreateLine("UpperHead", arrow, new Vector2(16f, 0f), new Vector2(11f, 2.5f));
        RectTransform lower = CreateLine("LowerHead", arrow, new Vector2(16f, 0f), new Vector2(11f, 2.5f));
        upper.anchorMin = upper.anchorMax = lower.anchorMin = lower.anchorMax = new Vector2(0.5f, 0.5f);
        upper.pivot = lower.pivot = new Vector2(1f, 0.5f);
        upper.localEulerAngles = new Vector3(0f, 0f, 35f);
        lower.localEulerAngles = new Vector3(0f, 0f, -35f);
        foreach (Image line in arrow.GetComponentsInChildren<Image>())
            line.color = new Color(0.72f, 0.87f, 1f, 0.85f);

        JunFieldIndicator field = fieldRoot.gameObject.AddComponent<JunFieldIndicator>();
        SerializedObject fieldSettings = new SerializedObject(field);
        SetReference(fieldSettings, "player", player);
        SetReference(fieldSettings, "gameCamera", Camera.main);
        SetReference(fieldSettings, "fieldText", fieldLabel);
        SetReference(fieldSettings, "arrow", arrow);
        SetReference(fieldSettings, "shaft", shaft);
        SetReference(fieldSettings, "upperHead", upper);
        SetReference(fieldSettings, "lowerHead", lower);
        SetReference(fieldSettings, "group", fieldGroup);
        fieldSettings.FindProperty("worldOffset").vector3Value = new Vector3(0f, -visual.bounds.extents.y - 0.45f, 0f);
        fieldSettings.ApplyModifiedProperties();
        fieldRoot.gameObject.SetActive(true);
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(player.transform.position +
            fieldSettings.FindProperty("worldOffset").vector3Value);
        fieldRoot.position = new Vector3(screenPosition.x, screenPosition.y, 0f);

        if (!AssetDatabase.IsValidFolder("Assets/Work/Jun/Materials"))
            AssetDatabase.CreateFolder("Assets/Work/Jun", "Materials");
        string materialPath = "Assets/Work/Jun/Materials/LandingGlow.mat";
        Material glow = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (glow == null)
        {
            glow = new Material(font.material);
            glow.name = "LandingGlow";
            glow.shader = Shader.Find("TextMeshPro/Distance Field");
            glow.EnableKeyword("GLOW_ON");
            glow.SetColor("_GlowColor", new Color(1f, 0.82f, 0.16f, 0.35f));
            glow.SetFloat("_GlowInner", 0.02f);
            glow.SetFloat("_GlowOuter", 0.12f);
            glow.SetFloat("_GlowPower", 0.15f);
            glow.SetFloat("_OutlineWidth", 0.08f);
            glow.SetColor("_OutlineColor", new Color(0.04f, 0.035f, 0.02f, 0.85f));
            AssetDatabase.CreateAsset(glow, materialPath);
        }

        RectTransform landing = CreateRect("LandingFeedback", hud.transform, new Vector2(0.5f, 0.64f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 150f));
        landing.gameObject.SetActive(false);
        CanvasGroup landingGroup = landing.gameObject.AddComponent<CanvasGroup>();
        landingGroup.blocksRaycasts = false;
        landingGroup.interactable = false;
        landingGroup.alpha = 0f;
        TMP_Text judgement = CreateText("Judgement", landing, font, "완벽!", 70f,
            new Color(1f, 0.82f, 0.16f), Vector2.zero, new Vector2(620f, 95f), true);
        judgement.fontStyle = FontStyles.Bold;
        judgement.fontSharedMaterial = glow;
        judgement.extraPadding = true;
        judgement.UpdateMeshPadding();
        TMP_Text detail = CreateText("Detail", landing, font, "콤보 x1", 20f,
            LabelColor, new Vector2(0f, -96f), new Vector2(620f, 32f), true);

        JunLandingFeedback feedback = landing.gameObject.AddComponent<JunLandingFeedback>();
        SerializedObject feedbackSettings = new SerializedObject(feedback);
        SetReference(feedbackSettings, "player", player);
        SetReference(feedbackSettings, "feedbackRoot", landing);
        SetReference(feedbackSettings, "judgementText", judgement);
        SetReference(feedbackSettings, "detailText", detail);
        SetReference(feedbackSettings, "group", landingGroup);
        feedbackSettings.ApplyModifiedProperties();
        landing.gameObject.SetActive(true);

        foreach (Work.KJS._00.Scripts.OnPerfectEffect oldEffect in
            Object.FindObjectsByType<Work.KJS._00.Scripts.OnPerfectEffect>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Undo.RecordObject(oldEffect, "Replace old landing feedback");
            oldEffect.enabled = false;
            Undo.RecordObject(oldEffect.gameObject, "Hide old landing text");
            oldEffect.gameObject.SetActive(false);
        }

        Undo.RegisterCreatedObjectUndo(fieldRoot.gameObject, "Create field indicator");
        Undo.RegisterCreatedObjectUndo(landing.gameObject, "Create landing feedback");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = landing.gameObject;
        Debug.Log("Jun 잔상, E 표시, PERFECT/GOOD/BAD 착지 피드백 연결 완료.");
    }
    // Main 씬에 속도 연출과 카메라 효과 연결
    [MenuItem("Tools/Jun/Build Speed Effects")]
    public static void BuildSpeedEffects()
    {
        if (Application.isPlaying || EditorSceneManager.GetActiveScene().path != MainScenePath)
        {
            Debug.LogError("재생을 중지하고 Jun/Main 씬에서 실행해 주세요.");
            return;
        }

        if (GameObject.Find("JunSpeedEffects") != null)
        {
            Debug.Log("속도 연출이 이미 있습니다. JunSpeedEffects에서 조절해 주세요.");
            return;
        }

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        GameState gameState = Object.FindFirstObjectByType<GameState>();
        Unity.Cinemachine.CinemachineCamera virtualCamera =
            Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        Unity.Cinemachine.CinemachineCameraOffset cameraOffset =
            virtualCamera.GetComponent<Unity.Cinemachine.CinemachineCameraOffset>();
        if (cameraOffset == null)
        {
            cameraOffset = Undo.AddComponent<Unity.Cinemachine.CinemachineCameraOffset>(virtualCamera.gameObject);
            cameraOffset.ApplyAfter = Unity.Cinemachine.CinemachineCore.Stage.Noise;
        }

        string materialPath = "Assets/Work/Jun/Materials/WindLines.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Sprites/Default"));
            material.name = "WindLines";
            AssetDatabase.CreateAsset(material, materialPath);
        }

        GameObject effectsRoot = new GameObject("JunSpeedEffects");
        effectsRoot.SetActive(false);
        GameObject windRoot = new GameObject("WindLines");
        windRoot.transform.SetParent(effectsRoot.transform, false);
        SpriteRenderer playerSprite = player.GetComponentInChildren<SpriteRenderer>();
        LineRenderer[] lines = new LineRenderer[36];
        for (int i = 0; i < lines.Length; i++)
        {
            GameObject lineObject = new GameObject("WindLine" + i, typeof(LineRenderer));
            lineObject.transform.SetParent(windRoot.transform, false);
            LineRenderer line = lineObject.GetComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.sortingLayerID = playerSprite.sortingLayerID;
            line.sortingOrder = -10;
            line.enabled = false;
            lines[i] = line;
        }

        JunWindLines wind = windRoot.AddComponent<JunWindLines>();
        SerializedObject windSettings = new SerializedObject(wind);
        SetReference(windSettings, "player", player);
        SetReference(windSettings, "gameState", gameState);
        SetReference(windSettings, "gameCamera", Camera.main);
        SerializedProperty lineArray = windSettings.FindProperty("lines");
        lineArray.arraySize = lines.Length;
        for (int i = 0; i < lines.Length; i++)
            lineArray.GetArrayElementAtIndex(i).objectReferenceValue = lines[i];
        windSettings.ApplyModifiedProperties();

        JunCameraEffects cameraEffects = effectsRoot.AddComponent<JunCameraEffects>();
        SerializedObject cameraSettings = new SerializedObject(cameraEffects);
        SetReference(cameraSettings, "player", player);
        SetReference(cameraSettings, "gameState", gameState);
        SetReference(cameraSettings, "virtualCamera", virtualCamera);
        SetReference(cameraSettings, "cameraOffset", cameraOffset);
        cameraSettings.ApplyModifiedProperties();

        effectsRoot.SetActive(true);
        Undo.RegisterCreatedObjectUndo(effectsRoot, "Create speed effects");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = effectsRoot;
        Debug.Log("Jun 바람선, 속도별 줌, PERFECT/GOOD/BAD 착지 흔들림 연결 완료.");
    }
    // Main 씬에 카운트다운과 결과 화면 연결
    [MenuItem("Tools/Jun/Build Session UI")]
    public static void BuildSessionUI()
    {
        if (Application.isPlaying || EditorSceneManager.GetActiveScene().path != MainScenePath)
        {
            Debug.LogError("재생을 중지하고 Jun/Main 씬에서 실행해 주세요.");
            return;
        }

        GameObject hud = GameObject.Find("JunHUD");
        if (hud == null || hud.transform.Find("SessionUI") != null)
        {
            Debug.LogError("JunHUD가 없거나 진행 UI가 이미 있습니다.");
            return;
        }

        if (hud.GetComponent<GraphicRaycaster>() == null)
            Undo.AddComponent<GraphicRaycaster>(hud);
        TMP_FontAsset font = hud.GetComponentInChildren<TMP_Text>(true).font;
        RectTransform root = CreateRect("SessionUI", hud.transform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
        root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;
        root.gameObject.SetActive(false);

        RectTransform countdown = CreateRect("Countdown", root, new Vector2(0.5f, 0.55f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 200f));
        CanvasGroup countdownGroup = countdown.gameObject.AddComponent<CanvasGroup>();
        countdownGroup.blocksRaycasts = false;
        countdownGroup.interactable = false;
        CreateText("ReadyLabel", countdown, font, "준비", 14f, LabelColor,
            Vector2.zero, new Vector2(600f, 24f), true);
        TMP_Text count = CreateText("Count", countdown, font, "2", 96f, ValueColor,
            new Vector2(0f, -26f), new Vector2(600f, 125f), true);
        count.fontStyle = FontStyles.Bold;
        TMP_Text hint = CreateText("Controls", countdown, font, "스페이스: 누르면 하강 / 떼면 비행",
            17f, LabelColor, new Vector2(0f, -164f), new Vector2(600f, 28f), true);

        RectTransform overlay = CreateRect("ResultsOverlay", root, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = overlay.offsetMax = Vector2.zero;
        Image backdrop = overlay.gameObject.AddComponent<Image>();
        backdrop.color = new Color(0.01f, 0.02f, 0.04f, 0.76f);
        CanvasGroup resultsGroup = overlay.gameObject.AddComponent<CanvasGroup>();
        RectTransform card = CreateRect("ResultsCard", overlay, new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880f, 640f));
        Image cardBackground = card.gameObject.AddComponent<Image>();
        cardBackground.color = new Color(0.025f, 0.045f, 0.075f, 0.97f);
        cardBackground.raycastTarget = false;
        TMP_Text title = CreateText("Title", card, font, "도전 결과", 38f, ValueColor,
            new Vector2(0f, -28f), new Vector2(880f, 54f), true);
        title.fontStyle = FontStyles.Bold;
        CreateText("DistanceLabel", card, font, "이동 거리", 13f, LabelColor,
            new Vector2(0f, -108f), new Vector2(880f, 22f), true);
        TMP_Text distance = CreateText("Distance", card, font, "0 m", 76f, ValueColor,
            new Vector2(0f, -135f), new Vector2(880f, 94f), true);
        CreateLine("Divider", card, new Vector2(100f, -248f), new Vector2(680f, 1f));
        TMP_Text speed = CreateResultStat(card, font, "TopSpeed", "최고 속도", new Vector2(100f, -280f));
        TMP_Text delta = CreateResultStat(card, font, "MaxDeltaV", "최대 전위차", new Vector2(480f, -280f));
        TMP_Text perfect = CreateResultStat(card, font, "PerfectCount", "완벽한 착지", new Vector2(100f, -388f));
        TMP_Text combo = CreateResultStat(card, font, "MaxCombo", "최고 콤보", new Vector2(480f, -388f));

        RectTransform restartRect = CreateRect("RestartButton", card, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(280f, -514f), new Vector2(320f, 60f));
        Image restartImage = restartRect.gameObject.AddComponent<Image>();
        restartImage.color = new Color(0.1f, 0.29f, 0.34f);
        Button restart = restartRect.gameObject.AddComponent<Button>();
        restart.targetGraphic = restartImage;
        TMP_Text restartLabel = CreateText("Label", restartRect, font, "다시 시작", 20f, ValueColor,
            Vector2.zero, new Vector2(320f, 60f), true);
        restartLabel.fontStyle = FontStyles.Bold;
        CreateText("RestartHint", card, font, "R 키를 눌러 다시 시작", 13f, LabelColor,
            new Vector2(0f, -588f), new Vector2(880f, 24f), true);
        overlay.gameObject.SetActive(false);

        JunSessionUI session = root.gameObject.AddComponent<JunSessionUI>();
        SerializedObject settings = new SerializedObject(session);
        SetReference(settings, "player", Object.FindFirstObjectByType<PlayerController>());
        SetReference(settings, "gameState", Object.FindFirstObjectByType<GameState>());
        SetReference(settings, "potentialTracker", Object.FindFirstObjectByType<PotentialTracker>());
        SetReference(settings, "countdownText", count);
        SetReference(settings, "countdownHint", hint);
        SetReference(settings, "countdownGroup", countdownGroup);
        SetReference(settings, "resultsPanel", overlay.gameObject);
        SetReference(settings, "resultsGroup", resultsGroup);
        SetReference(settings, "distanceText", distance);
        SetReference(settings, "topSpeedText", speed);
        SetReference(settings, "maxDeltaText", delta);
        SetReference(settings, "perfectText", perfect);
        SetReference(settings, "maxComboText", combo);
        SetReference(settings, "restartButton", restart);
        string[] hiddenNames = { "TopHUD", "RightHUD", "PotentialAxis", "PotentialTitle", "FieldIndicator", "LandingFeedback" };
        SerializedProperty hidden = settings.FindProperty("gameplayElements");
        hidden.arraySize = hiddenNames.Length;
        for (int i = 0; i < hiddenNames.Length; i++)
            hidden.GetArrayElementAtIndex(i).objectReferenceValue = hud.transform.Find(hiddenNames[i]).gameObject;
        settings.ApplyModifiedProperties();
        root.gameObject.SetActive(true);

        Undo.RegisterCreatedObjectUndo(root.gameObject, "Create session UI");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = root.gameObject;
        Debug.Log("Jun 2-1-GO 카운트다운, 결과 5종, 현재 씬 재시작 버튼과 R 키 연결 완료.");
    }

    // 결과 항목의 라벨과 수치 생성
    private static TMP_Text CreateResultStat(RectTransform parent, TMP_FontAsset font, string name, string label, Vector2 position)
    {
        CreateText(name + "Label", parent, font, label, 13f, LabelColor, position, new Vector2(300f, 24f));
        return CreateText(name, parent, font, "0", 34f, ValueColor,
            position + new Vector2(0f, -28f), new Vector2(300f, 52f));
    }
    // 월드 바람선 연결과 종료 시 잔여 효과 유지
    [MenuItem("Tools/Jun/Build World Speed Lines")]
    public static void BuildUISpeedLines()
    {
        if (Application.isPlaying || EditorSceneManager.GetActiveScene().path != MainScenePath)
        {
            Debug.LogError("재생을 중지하고 Jun/Main 씬에서 실행해 주세요.");
            return;
        }

        GameObject effects = GameObject.Find("JunSpeedEffects");
        if (effects == null) effects = new GameObject("JunSpeedEffects");
        SpeedLines speedLines = Object.FindFirstObjectByType<SpeedLines>();
        if (speedLines == null)
        {
            GameObject item = new GameObject("SpeedLines");
            speedLines = item.AddComponent<SpeedLines>();
            Undo.RegisterCreatedObjectUndo(item, "Create world speed lines");
        }
        Undo.SetTransformParent(speedLines.transform, effects.transform, "Place world speed lines");
        speedLines.transform.localPosition = Vector3.zero;
        speedLines.transform.localRotation = Quaternion.identity;
        speedLines.transform.localScale = Vector3.one;
        speedLines.gameObject.layer = 0;

        SerializedObject settings = new SerializedObject(speedLines);
        SetReference(settings, "player", Object.FindFirstObjectByType<PlayerController>());
        SetReference(settings, "gameState", Object.FindFirstObjectByType<GameState>());
        SetReference(settings, "gameCamera", Camera.main);
        SetReference(settings, "lineMaterial", AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Work/Jun/Materials/WindLines.mat"));
        settings.ApplyModifiedProperties();

        JunSessionUI session = Object.FindFirstObjectByType<JunSessionUI>();
        Undo.RecordObject(session, "Keep wind fade on results");
        SerializedObject sessionSettings = new SerializedObject(session);
        SerializedProperty gameplayElements = sessionSettings.FindProperty("gameplayElements");
        for (int i = gameplayElements.arraySize - 1; i >= 0; i--)
        {
            if (gameplayElements.GetArrayElementAtIndex(i).objectReferenceValue != speedLines.gameObject)
                continue;
            gameplayElements.GetArrayElementAtIndex(i).objectReferenceValue = null;
            gameplayElements.DeleteArrayElementAtIndex(i);
        }
        sessionSettings.ApplyModifiedProperties();

        Transform oldWind = effects.transform.Find("WindLines");
        if (oldWind != null) oldWind.gameObject.SetActive(false);
        speedLines.gameObject.SetActive(true);
        EditorUtility.SetDirty(speedLines);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("월드 바람선 연결 완료: 감속과 종료 때 남은 선은 서서히 사라집니다.");
    }
    // UI 영역 생성
    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        item.layer = 5;
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    // HUD 텍스트 생성
    private static TMP_Text CreateText(string name, RectTransform parent, TMP_FontAsset font, string text,
        float size, Color color, Vector2 position, Vector2 dimensions, bool centered = false)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), position, dimensions);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = centered ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        return label;
    }

    // 눈금과 구분선 생성
    private static RectTransform CreateLine(string name, RectTransform parent, Vector2 position, Vector2 size)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.75f, 0.83f, 0.9f, 0.28f);
        image.raycastTarget = false;
        return rect;
    }

    // Inspector 참조 연결
    private static void SetReference(SerializedObject settings, string fieldName, Object value)
    {
        settings.FindProperty(fieldName).objectReferenceValue = value;
    }
}








