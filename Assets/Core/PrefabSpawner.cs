using System;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField] private float _baseCooldown;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _spawnPointsContainer;

    private float _currentCooldown;
    private int _spawnPointIndex;
    private Transform[] _spawnPoints = null!;

    private void Start()
    {
        _spawnPoints = new Transform[_spawnPointsContainer.childCount];

        for (int i = 0; i < _spawnPointsContainer.childCount; i++)
            _spawnPoints[i] = _spawnPointsContainer.GetChild(i);
    }

    private void Update()
    {
        if (_spawnPoints.Length == 0)
            return;

        _currentCooldown = Math.Max(0, _currentCooldown - Time.deltaTime);

        if (_currentCooldown != 0)
            return;

        Instantiate(
            _prefab,
            _spawnPoints[_spawnPointIndex].transform.position,
            Quaternion.identity
        );
        _currentCooldown = _baseCooldown;
        _spawnPointIndex++;

        if (_spawnPointIndex >= _spawnPoints.Length)
            _spawnPointIndex = 0;
    }
}