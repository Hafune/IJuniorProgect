using System;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float _baseCooldown;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _spawnPointsContainer;

    private int _spawnPointIndex;
    private Transform[] _spawnPoints = null!;

    private void Start()
    {
        _spawnPoints = new Transform[_spawnPointsContainer.childCount];

        for (int i = 0; i < _spawnPointsContainer.childCount; i++)
            _spawnPoints[i] = _spawnPointsContainer.GetChild(i);

        StartCoroutine(PressAnimation());
    }

    private IEnumerator PressAnimation()
    {
        if (_spawnPoints.Length == 0)
            yield break;

        Instantiate(
            _prefab,
            _spawnPoints[_spawnPointIndex].transform.position,
            Quaternion.identity
        );
        _spawnPointIndex++;

        yield return new WaitForSeconds(_baseCooldown);

        if (_spawnPointIndex >= _spawnPoints.Length)
            _spawnPointIndex = 0;

        StartCoroutine(PressAnimation());
    }
}