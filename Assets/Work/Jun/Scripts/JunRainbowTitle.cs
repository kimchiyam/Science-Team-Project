using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class JunRainbowTitle : MonoBehaviour
{
    [SerializeField] private Gradient rainbow = new Gradient();

    private TMP_Text title;

    // 제목을 다시 그릴 때 그라데이션 적용
    private void OnEnable()
    {
        title = GetComponent<TMP_Text>();
        title.OnPreRenderText += ApplyRainbow;
        title.SetVerticesDirty();
    }

    // 제목 색상 연결 해제
    private void OnDisable()
    {
        title.OnPreRenderText -= ApplyRainbow;
        title.SetVerticesDirty();
    }

    // Inspector에서 바꾼 색상 반영
    private void OnValidate()
    {
        if (title != null) title.SetVerticesDirty();
    }

    // 문장 전체의 가로 위치에 따라 무지개 색 적용
    private void ApplyRainbow(TMP_TextInfo textInfo)
    {
        float left = float.MaxValue;
        float right = float.MinValue;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;
            left = Mathf.Min(left, textInfo.characterInfo[i].bottomLeft.x);
            right = Mathf.Max(right, textInfo.characterInfo[i].topRight.x);
        }
        float width = Mathf.Max(0.01f, right - left);
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];
            if (!textInfo.characterInfo[i].isVisible) continue;
            TMP_MeshInfo mesh = textInfo.meshInfo[character.materialReferenceIndex];
            for (int corner = 0; corner < 4; corner++)
            {
                int vertex = character.vertexIndex + corner;
                float position = Mathf.Clamp01((mesh.vertices[vertex].x - left) / width);
                mesh.colors32[vertex] = rainbow.Evaluate(position);
            }
        }
    }
}

