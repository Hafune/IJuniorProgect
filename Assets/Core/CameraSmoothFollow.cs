using System;
using UnityEngine;

public class CameraSmoothFollow : MonoBehaviour
{
    [SerializeField] private Transform target = null!;

    private Vector3 _offset = new Vector3(0f, 0f, -10f);
    private float _smoothTime = 5f;

    private void Update()
    {
        var targetPosition = target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothTime * Time.deltaTime);
    }
}