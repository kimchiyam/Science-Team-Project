using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapCreateManager : MonoBehaviour
{
    [SerializeField]private List<Vector2> pointsList = new();
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private LineRenderer lineRenderer;
    private AnimationCurve curve = new();

    [SerializeField] private int linefromlinePointCount = 50;

    private void Start()
    {
        CreatePoints();
    }

    private void CreatePoints()
    {
        
        foreach (var p in pointsList)
        {
            GameObject point = Instantiate(pointPrefab, p,Quaternion.identity);
            curve.AddKey(new Keyframe(p.x, p.y, 0, 0));
        }
        
        float start = curve.keys[0].time;
        float end= curve.keys[curve.length - 1].time;
        
        float step = (end - start) / (linefromlinePointCount - 1);

        lineRenderer.positionCount = linefromlinePointCount;

        for (int i = 0; i < linefromlinePointCount; i++)
        {
            float x = start + i * step;
            float y = curve.Evaluate(x);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            Debug.Log($"{i} {x} {y}");
        }
        
    }
}
