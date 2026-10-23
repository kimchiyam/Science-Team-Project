using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MapCreateManager : MonoBehaviour
{
    public static MapCreateManager instance;
    [SerializeField]private List<Vector2> pointsList;
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private GameObject arrow;
    [SerializeField] private float arrowDistance;
    

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private EdgeCollider2D edgeCollider;
    
    public AnimationCurve curve = new();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
    }


    [SerializeField] private int linefromlinePointCount = 50;
    [SerializeField] private int CreatePointCount;
    [SerializeField] private int minimumPointDistance = 8;
    [SerializeField] private int maximumPointDistance = 15;
    private int _currentX;
    private int _currentY;
    private bool _isLowY;

    private void Start()
    {
        CreatePoints();
    }

    public float GetSlope(float x)
    {
         float right = curve.Evaluate(x + 0.01f);
        float left  = curve.Evaluate(x - 0.01f);
        return (right - left) / 0.02f;
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

            int value = Random.Range(minimumPointDistance, maximumPointDistance);
            _currentX += value;
            if (!_isLowY)
            {
                _currentY = Random.Range(6, 10);
                _isLowY = true;
            }
            else
            {
                _currentY = Random.Range(0, 4);
                _isLowY = false;
            }

            pointsList.Add(new Vector2(_currentX, _currentY));



        }

        linefromlinePointCount = pointsList.Count * 100;

        foreach (var p in pointsList)
        {
            GameObject point = Instantiate(pointPrefab, p, Quaternion.identity);
            curve.AddKey(new Keyframe(p.x, p.y, 0, 0));
        }

        float start = curve.keys[0].time;
        float end = curve.keys[curve.length - 1].time;
        for (float x = start; x <= end; x += arrowDistance)
        {
            float slop = GetSlope(x);
            float dirX = slop < 0 ? 1f : -1f;
            float dirY = slop * dirX;
            float angle = Mathf.Atan2(dirY,dirX) * Mathf.Rad2Deg;
           GameObject arro = Instantiate(arrow, new Vector3(x, curve.Evaluate(x)+0.5f, 0), Quaternion.Euler(0,0,angle));
           SpriteRenderer[] arrosp =  arro.GetComponentsInChildren<SpriteRenderer>();
           foreach (var sr in arrosp)
           {
               if (dirX > 0)
               {
                   sr.color = Color.red;
               }
               else
               {
                   sr.color = Color.blue;
               }
           }
                
           
        }


    float step = (end - start) / (linefromlinePointCount - 1);

        lineRenderer.positionCount = linefromlinePointCount;
        
        List<Vector2> points = new();

        for (int i = 0; i < linefromlinePointCount; i++)
        {
            float x = start + i * step;
            float y = curve.Evaluate(x);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            points.Add(new Vector2(x, y));
        }

        edgeCollider.SetPoints(points);


    }
}


