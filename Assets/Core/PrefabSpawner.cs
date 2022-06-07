using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _prefab = null!;
    [SerializeField] private Transform _spawnPointsContainer = null!;

    private int _spawnPointIndex;
    private Transform[] _spawnPoints = null!;

    private void Start()
    {
        _spawnPoints = new Transform[_spawnPointsContainer.childCount];

        for (int i = 0; i < _spawnPointsContainer.childCount; i++)
            _spawnPoints[i] = _spawnPointsContainer.GetChild(i);

        SpawnPrefab();
    }

    private void SpawnPrefab()
    {
        if (_spawnPoints.Length == 0)
            return;

        Instantiate(
            _prefab,
            _spawnPoints[_spawnPointIndex++].transform.position,
            Quaternion.identity
        );

        if (_spawnPointIndex >= _spawnPoints.Length)
        {
            Destroy(gameObject);
            return;
        }

        SpawnPrefab();
    }
}