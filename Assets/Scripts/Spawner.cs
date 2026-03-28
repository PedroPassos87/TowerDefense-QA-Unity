using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static event Action<int> OnWaveChanged;

    [Header("Waves")]
    [SerializeField] private WaveData[] waves;
    private int _currentWaveIndex = 0;
    private WaveData CurrentWave => waves[_currentWaveIndex];
    private int _waveCounter = 0;
    private float _timeBetweenWaves = 1f; //verificar depois para trocar por um botao
    private float _waveCooldown;
    private bool _isBetweenWaves = false;

    [Header("Spawn Settings")]
    private float _spawnTimer;
    private float _spawnCounter;
    private int _enemiesRemoved;

    [Header("Pools")]
    [SerializeField] private ObjectPooler Enemy1Pool;
    [SerializeField] private ObjectPooler Enemy2Pool;
    [SerializeField] private ObjectPooler Enemy3Pool;

    private Dictionary<EnemyType, ObjectPooler> _poolDictionary;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>()
        {
          {EnemyType.Enemy1, Enemy1Pool},  
          {EnemyType.Enemy2, Enemy2Pool},  
          {EnemyType.Enemy3, Enemy3Pool},  
        };
    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;

    }

    private void Start()
    {
        OnWaveChanged?.Invoke(_waveCounter);

    }

    void Update()
    {
        if (_isBetweenWaves)
        {
            _waveCooldown -= Time.deltaTime;
            if (_waveCooldown <= 0)
            {
                _currentWaveIndex = (_currentWaveIndex + 1) % waves.Length; //olhar essa questao pq parece loop
                _waveCounter++;
                OnWaveChanged?.Invoke(_waveCounter);
                _spawnCounter = 0;
                _enemiesRemoved = 0;
                _spawnTimer = 0f;
                _isBetweenWaves = false;
            }
        }
        else
        {    
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 && _spawnCounter < CurrentWave.enemiesPerWave)
            {
                _spawnTimer = CurrentWave.spawnInterval;
                SpawnEnemy();
                _spawnCounter++;
            }else if(_spawnCounter >= CurrentWave.enemiesPerWave && _enemiesRemoved >= CurrentWave.enemiesPerWave)
            {
                _isBetweenWaves = true;
                _waveCooldown = _timeBetweenWaves;
            }
        }
    }

    private void SpawnEnemy()
    {
        if (_poolDictionary.TryGetValue(CurrentWave.enemyType, out var pool))
        {
            GameObject spawnedObject = pool.GetPooledObject();
            spawnedObject.transform.position = transform.position;

            float healthMultiplier = 1f + (_waveCounter * 0.05f);
            Enemy enemy = spawnedObject.GetComponent<Enemy>();
            enemy.Initialize(healthMultiplier);
            
            spawnedObject.SetActive(true);
            
        }

    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _enemiesRemoved++;
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesRemoved++;
    }
}
