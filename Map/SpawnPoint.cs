using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace OnePercent
{
    public class SpawnPoint : MonoBehaviour
    {
        private MapInfo _mapInfo;

        [SerializeField]
        List<string> _spawnUnitList = new List<string>();
        public List<string> SpawnUnitList
        {
            get { return _spawnUnitList; }
        }

        [SerializeField]
        private float _spawnInterval = 5f;
        private int _maxSpawnCount = 10;

        private List<CharacterBase> _currentSpawnUnitList = new List<CharacterBase>();

        public float SpawnInterval
        {
            get { return _spawnInterval; }
        }

        private bool _isSpawnable = true;
        public bool IsSpawnable
        {
            get { return _isSpawnable; }
        }

        private IEnumerator _spawnProcessCo;

        public void Init(MapInfo mapInfo)
        {
            _mapInfo = mapInfo;
            SpawnStart();
        }

        public void SpawnStart()
        {
            if (_isSpawnable)
            {
                if (_spawnProcessCo != null)
                {
                    StopCoroutine(_spawnProcessCo);
                    _spawnProcessCo = null;
                }
                _spawnProcessCo = SpawnProcessCo();
                StartCoroutine(_spawnProcessCo);
            }
        }

        private IEnumerator SpawnProcessCo()
        {
            var currentSpawnTime = 0f;
            while (true)
            {
                if (_currentSpawnUnitList.Count >= _maxSpawnCount)
                {
                    yield return null;
                }

                currentSpawnTime += Time.deltaTime;
                if (currentSpawnTime >= _spawnInterval)
                {
                    Spawn();
                    currentSpawnTime = 0f;
                }
                yield return null;
            }
        }

        private void Spawn()
        {
            for (int i = 0; i < _spawnUnitList.Count; i++)
            {
                var unitName = _spawnUnitList[i];
                var unit = _mapInfo.GetPooledObject(unitName);
                unit.Init();
                unit.transform.position = transform.position;
                unit.gameObject.SetActive(true);

                _currentSpawnUnitList.Add(unit);
                unit.OnDieEvent += () => RemoveQueue(unit);
            }
        }

        private void RemoveQueue(CharacterBase unit)
        {
            _currentSpawnUnitList.Remove(unit);
        }
    }
}
