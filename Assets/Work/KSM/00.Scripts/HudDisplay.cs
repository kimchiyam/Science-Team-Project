using TMPro;
using UnityEngine;

public class HudDisplay : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PotentialTracker tracker;
 
    [Header("UI 텍스트")]
    [SerializeField] private TMP_Text distanceText; 
    [SerializeField] private TMP_Text valueText;    
 
    [Header("단위")]
    [Tooltip("땅 높이 1 = 몇 V로 보여 줄지")]
    [SerializeField] private float voltsPerUnit = 10f;
 
    private void Update()
    {
        if (player == null || MapCreateManager.instance == null) return;
        float x = player.transform.position.x;
        float v = MapCreateManager.instance.curve.Evaluate(x) * voltsPerUnit;
        float dv = tracker != null ? tracker._deltaV * voltsPerUnit : 0f;
        float e = Mathf.Abs(MapCreateManager.instance.GetSlope(x)) * voltsPerUnit;
        float speed = player.Speed;
        distanceText.SetText("{0:0} m", Mathf.Max(0f, player.Distance));
        valueText.SetText("{0:0} V\n{1:0} V\n{2:1} V/m\n{3:1} m/s", v, dv, e, speed);
        
    }
}
