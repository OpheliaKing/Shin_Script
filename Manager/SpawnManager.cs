using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnePercent
{
    public class SpawnManager : ManagerBase
    {
        private MapInfo _currentMapInfo;

        public void Init(string mapName)
        {
            CreateMapInfo(mapName);
            PlayerSpawn(_currentMapInfo.PlayerSpawnPoint);
        }

        private void CreateMapInfo(string mapName)
        {
            _currentMapInfo = Instantiate(GameManager.Instance.ResourceManager.LoadPrefab<MapInfo>("Map/" + mapName));
            _currentMapInfo.Init();
        }

        private void PlayerSpawn(Vector3 spawnPoint)
        {
            var player = GameManager.Instance.ResourceManager.LoadPrefabAndInstantiate<PlayerUnit>("Unit/Player");
            GameManager.Instance.InputController.SetTargetUnit(player);
            player.Init();
            GameManager.Instance.CombatManager.SetPlayerUnit(player);
            player.transform.position = spawnPoint;            
        }
    }
}