using System;
using UnityEngine;

public class PotentialTracker : MonoBehaviour
{ 
  [SerializeField] private Transform target;
  private float _beforeHigh;
  private float _maxHigh;
  private float _deltaVMax;
  public float _deltaV;
  private float  _currentHigh;

  private void Awake()
  {
     
      
  }

  private void Update()
  {
   _currentHigh = MapCreateManager.instance.curve.Evaluate(target.position.x);
   
   if(_currentHigh > _beforeHigh)
       _maxHigh = _currentHigh;

   _deltaV = _maxHigh - _currentHigh;
   
   if (_deltaV > _deltaVMax)
       _deltaVMax = _deltaV;
   
   _beforeHigh = _currentHigh;
   Debug.Log($"{_deltaV} {_deltaVMax}");
  }
}
