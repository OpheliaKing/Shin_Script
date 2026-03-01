using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace OnePercent
{
    public class GameManager : Singleton<GameManager>
    {
        private List<ManagerBase> _managers = new List<ManagerBase>();

        private ResourceManager _resourceManager;
        public ResourceManager ResourceManager
        {
            get
            {
                if (_resourceManager == null)
                {
                    _resourceManager = GetManager<ResourceManager>();
                }
                return _resourceManager;
            }
        }

        private InputController _inputController;
        public InputController InputController
        {
            get
            {
                if (_inputController == null)
                {
                    var count = transform.childCount;
                    for (int i = 0; i < count; i++)
                    {
                        var child = transform.GetChild(i);
                        if (child.GetComponent<InputController>() != null)
                        {
                            _inputController = child.GetComponent<InputController>();
                            break;
                        }
                    }
                }
                return _inputController;
            }
        }

        private CombatManager _combatManager;
        public CombatManager CombatManager
        {
            get
            {
                if (_combatManager == null)
                {
                    _combatManager = GetManager<CombatManager>();
                }
                return _combatManager;
            }
        }

        private SpawnManager _spawnManager;
        public SpawnManager SpawnManager
        {
            get
            {
                if (_spawnManager == null)
                {
                    _spawnManager = GetManager<SpawnManager>();
                }
                return _spawnManager;
            }
        }
        private UIManager _uiManager;
        public UIManager UIManager
        {
            get
            {
                if (_uiManager == null)
                {
                    _uiManager = GetManager<UIManager>();
                }
                return _uiManager;
            }
        }

        private Camera _camera;
        public Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = FindObjectOfType<Camera>();
                }
                return _camera;
            }
        }

        private T GetManager<T>() where T : ManagerBase
        {
            var count = transform.childCount;
            for (int i = 0; i < count; i++)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<T>() != null)
                {
                    return child.GetComponent<T>();
                }
            }
            return null;
        }

        protected override void Awake()
        {
            base.Awake();
            LoadManagers();
        }

        private void LoadManagers()
        {
            var count = transform.childCount;
            for (int i = 0; i < count; i++)
            {
                var child = transform.GetChild(i);
                var manager = child.GetComponent<ManagerBase>();
                if (manager != null)
                {
                    _managers.Add(manager);
                }
            }
        }

        private void LoadEndEvent()
        {

        }

        private void OnClickGameStart()
        {

        }

        public void GameStart()
        {
            SpawnManager.Init("Map_0001");
        }
    }
}
