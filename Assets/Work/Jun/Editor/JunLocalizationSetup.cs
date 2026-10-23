using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class JunLocalizationSetup
{
    // Main 씬의 사용자 표시 문구를 한국어로 변경
    [MenuItem("Tools/Jun/Apply Korean Text")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Work/Jun/Main.unity")
            throw new System.InvalidOperationException("재생을 중지하고 Jun/Main에서 실행해 주세요.");

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Work/Jun/Fonts/JunMenuFont.asset");
        Dictionary<string, string> translations = new Dictionary<string, string>
        {
            { "TIME LEFT", "남은 시간" },
            { "DISTANCE", "이동 거리" },
            { "FLIGHT DATA", "주행 정보" },
            { "SPEED", "속도" },
            { "ΔV", "전위차 ΔV" },
            { "E", "전기장 E" },
            { "POTENTIAL / V", "전위 / V" },
            { "PERFECT", "완벽!" },
            { "GOOD", "좋아요!" },
            { "BAD", "아쉬워요!" },
            { "x1 COMBO", "콤보 x1" },
            { "READY", "준비" },
            { "GO", "Start!" },
            { "HOLD SPACE TO DIVE  /  RELEASE TO FLY", "스페이스: 누르면 하강 / 떼면 비행" },
            { "RUN COMPLETE", "도전 결과" },
            { "TOP SPEED", "최고 속도" },
            { "MAX DELTA V", "최대 전위차" },
            { "PERFECT LANDINGS", "완벽한 착지" },
            { "BEST COMBO", "최고 콤보" },
            { "RESTART", "다시 시작" },
            { "PRESS R TO TRY AGAIN", "R 키를 눌러 다시 시작" },
            { "POTENTIAL SURFING", "전위 서핑" },
            { "SPACE / 마우스 클릭", "스페이스 / 마우스 클릭" },
            { "SPACE 또는 마우스 클릭으로 조작", "스페이스 또는 마우스 클릭으로 조작" }
        };

        TMP_Text[] texts = GameObject.Find("JunHUD").GetComponentsInChildren<TMP_Text>(true);
        string characters = "완벽!좋아요!아쉬워요!콤보x0123456789시작!";
        foreach (TMP_Text text in texts)
        {
            // 플레이어 아래 E 기호는 유지
            if (text.transform.parent.GetComponent<JunFieldIndicator>() != null)
                continue;
            if (translations.TryGetValue(text.text, out string translated))
            {
                Undo.RecordObject(text, "Translate UI text");
                text.text = translated;
            }
            if (!ContainsKorean(text.text)) continue;
            Undo.RecordObject(text, "Use Korean font");
            text.font = font;
            text.fontSharedMaterial = font.material;
            text.UpdateMeshPadding();
            float requiredHeight = text.GetPreferredValues().y + 4f;
            if (requiredHeight > text.rectTransform.rect.height)
                text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredHeight);
            EditorUtility.SetDirty(text);
            characters += text.text;
        }
        if (!font.HasCharacters(characters, out uint[] missingCodes, false, false)
            && !font.TryAddCharacters(characters, out string missing))
            throw new System.InvalidOperationException("한글 글꼴 누락: " + characters);

        JunLandingFeedback feedback = Object.FindFirstObjectByType<JunLandingFeedback>();
        var settings = new SerializedObject(feedback);
        TMP_Text judgement = (TMP_Text)settings.FindProperty("judgementText").objectReferenceValue;
        string glowPath = "Assets/Work/Jun/Materials/LandingGlowKorean.mat";
        Material glow = AssetDatabase.LoadAssetAtPath<Material>(glowPath);
        if (glow == null)
        {
            glow = new Material(font.material);
            glow.name = "LandingGlowKorean";
            glow.shader = Shader.Find("TextMeshPro/Distance Field");
            glow.EnableKeyword("GLOW_ON");
            glow.SetColor("_GlowColor", new Color(1f, 0.82f, 0.16f, 0.35f));
            glow.SetFloat("_GlowInner", 0.02f);
            glow.SetFloat("_GlowOuter", 0.12f);
            glow.SetFloat("_GlowPower", 0.15f);
            glow.SetFloat("_OutlineWidth", 0.08f);
            glow.SetColor("_OutlineColor", new Color(0.04f, 0.035f, 0.02f, 0.85f));
            AssetDatabase.CreateAsset(glow, glowPath);
        }
        judgement.fontSharedMaterial = glow;
        judgement.UpdateMeshPadding();
        EditorUtility.SetDirty(judgement);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // 한글 문구인지 확인
    private static bool ContainsKorean(string text)
    {
        foreach (char character in text)
            if (character >= '가' && character <= '힣') return true;
        return false;
    }
}




