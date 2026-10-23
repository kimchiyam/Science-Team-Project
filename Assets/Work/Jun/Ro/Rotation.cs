using System;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    private void Update()
    {
        transform.rotation *= Quaternion.Euler(0f, 0f, 0.5f);
    }
}
