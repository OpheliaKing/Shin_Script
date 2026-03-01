using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnePercent
{
    public class MapInfo : MonoBehaviour
    {
        [SerializeField]
        private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();

        [SerializeField]
        private Vector3 _playerSpawnPoint;
        public Vector3 PlayerSpawnPoint
        {
            get { return _playerSpawnPoint; }
        }

        private Dictionary<string, Queue<CharacterBase>> _pool = new Dictionary<string, Queue<CharacterBase>>();

        [SerializeField]
        private Transform _poolParent;

        public void Init()
        {
            for(int i = 0; i < _spawnPoints.Count; i++)
            {
                _spawnPoints[i].Init(this);
            }
        }

        private void CreatePool(string unitName, int createCount)
        {
            if (!_pool.ContainsKey(unitName))
            {
                _pool[unitName] = new Queue<CharacterBase>();
            }

            var prefab = GameManager.Instance.ResourceManager.LoadPrefab<CharacterBase>("Unit/" + unitName);
            for (int i = 0; i < createCount; i++)
            {
                var obj = Instantiate(prefab, _poolParent);
                obj.OnDieEndEvent += () => ReturnPooledObject(unitName, obj);
                obj.gameObject.SetActive(false);
                _pool[unitName].Enqueue(obj);
            }
        }

        public CharacterBase GetPooledObject(string unitName)
        {
            if (!_pool.ContainsKey(unitName))
            {
                CreatePool(unitName, 10);
            }

            if (_pool[unitName].Count == 0)
            {
                CreatePool(unitName, 10);
            }

            return _pool[unitName].Dequeue();
        }

        private void ReturnPooledObject(string unitName, CharacterBase obj)
        {
            if (!_pool.ContainsKey(unitName))
            {
                CreatePool(unitName, 10);
            }
            _pool[unitName].Enqueue(obj);
            obj.gameObject.SetActive(false);
            obj.transform.position = Vector3.zero;
            obj.transform.SetParent(_poolParent);
        }

    }
}
