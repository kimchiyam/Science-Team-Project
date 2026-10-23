using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class JunGradientSetup
{
    // 참고 이미지의 배경과 지형 그라데이션 연결
    [MenuItem("Tools/Jun/Apply Reference Gradient")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Work/Jun/Main.unity")
            throw new System.InvalidOperationException("재생을 중지하고 Jun/Main에서 실행해 주세요.");

        JunSceneStyle style = Object.FindFirstObjectByType<JunSceneStyle>();
        JunTerrainFill fill = style.GetComponent<JunTerrainFill>();
        if (fill == null) fill = Undo.AddComponent<JunTerrainFill>(style.gameObject);
        SerializedObject styleSettings = new SerializedObject(style);
        styleSettings.FindProperty("backgroundColor").colorValue = ParseColor("#09061C");
        styleSettings.FindProperty("groundColor").colorValue = ParseColor("#FFE2A4");
        styleSettings.FindProperty("groundWidth").floatValue = 0.055f;
        styleSettings.FindProperty("groundFill").objectReferenceValue = fill;
        styleSettings.ApplyModifiedProperties();

        SerializedObject settings = new SerializedObject(fill);
        settings.FindProperty("gameCamera").objectReferenceValue = Camera.main;
        settings.FindProperty("map").objectReferenceValue = Object.FindFirstObjectByType<MapCreateManager>();
        settings.FindProperty("groundLine").objectReferenceValue = styleSettings.FindProperty("groundLine").objectReferenceValue;
        settings.FindProperty("baseMaterial").objectReferenceValue = styleSettings.FindProperty("lineMaterial").objectReferenceValue;
        Gradient gradient = new Gradient();
        gradient.SetKeys(new[]
        {
            new GradientColorKey(ParseColor("#2D1362"), 0f),
            new GradientColorKey(ParseColor("#4F1879"), 0.2f),
            new GradientColorKey(ParseColor("#7E2083"), 0.45f),
            new GradientColorKey(ParseColor("#BC2F6D"), 0.68f),
            new GradientColorKey(ParseColor("#DE534D"), 0.86f),
            new GradientColorKey(ParseColor("#FF7A32"), 1f)
        }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        settings.FindProperty("groundGradient").gradientValue = gradient;
        settings.FindProperty("minimumHeight").floatValue = 0f;
        settings.FindProperty("maximumHeight").floatValue = 10f;
        settings.ApplyModifiedProperties();

        Camera.main.backgroundColor = styleSettings.FindProperty("backgroundColor").colorValue;
        LineRenderer line = (LineRenderer)styleSettings.FindProperty("groundLine").objectReferenceValue;
        line.startColor = line.endColor = styleSettings.FindProperty("groundColor").colorValue;
        line.startWidth = line.endWidth = styleSettings.FindProperty("groundWidth").floatValue;
        EditorUtility.SetDirty(Camera.main);
        EditorUtility.SetDirty(line);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // 참고 색상 코드 변환
    private static Color ParseColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }
}

