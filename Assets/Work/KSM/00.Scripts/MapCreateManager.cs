using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MapCreateManager : MonoBehaviour
{
    [SerializeField]private List<Vector2> pointsList;
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private EdgeCollider2D edgeCollider;
    
    private AnimationCurve curve = new();
    

    [SerializeField] private int linefromlinePointCount = 50;
    [SerializeField] private int CreatePointCount;
    private int _currentX;
    private int _currentY;
    private bool _isLowY;

    private void Start()
    {
        CreatePoints();
    }

    private void CreatePoints()
    {
        pointsList = new();
        for (int i = 0; i < CreatePointCount; i++)
        {
            if (i == 0)
            {
                pointsList.Add(new Vector2(0, 0));
                continue;
            }
            int value = Random.Range(8, 15);
            _currentX += value;
            if (!_isLowY)
            {
                _currentY = Random.Range(4, 8);
                _isLowY = true;
            }
            else
            {
                _currentY = Random.Range(0, 2);
                _isLowY = false;
            }
            pointsList.Add(new Vector2(_currentX, _currentY));
            
                
            
        }
        linefromlinePointCount = pointsList.Count*100;
        
        foreach (var p in pointsList)
        {
            GameObject point = Instantiate(pointPrefab, p,Quaternion.identity);
            curve.AddKey(new Keyframe(p.x, p.y, 0, 0));
        }
        
        float start = curve.keys[0].time;
        float end= curve.keys[curve.length - 1].time;
        
        float step = (end - start) / (linefromlinePointCount - 1);

        lineRenderer.positionCount = linefromlinePointCount;
        List<Vector2> points = new();

        for (int i = 0; i < linefromlinePointCount; i++)
        {
            float x = start + i * step;
            float y = curve.Evaluate(x);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            points.Add(new Vector2(x, y));
            Debug.Log($"{i} {x} {y}");
        }

        edgeCollider.SetPoints(points);


    }
}
