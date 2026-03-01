using System.Collections;
using System.Collections.Generic;
using OnePercent;
using UnityEngine;

namespace OnePercent
{
    public class UIManager : ManagerBase
    {
        private Canvas _canvas;
        public Canvas Canvas
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = FindObjectOfType<Canvas>();
                }
                return _canvas;
            }
        }

        private Queue<HealthUI> _healthUIPool = new Queue<HealthUI>();
        private HealthUI _healthUIPrefab;
        [SerializeField]
        private Transform _healthUIParent;

        private Queue<TamingUI> _tamingUIPool = new Queue<TamingUI>();
        private TamingUI _tamingUIPrefab;
        [SerializeField]
        private Transform _tamingUIParent;

        private void CreateHealthUIPool(int count)
        {
            if (_healthUIPrefab == null)
            {
                _healthUIPrefab = GameManager.Instance.ResourceManager.LoadPrefab<HealthUI>("UI/HealthUI");
            }

            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(_healthUIPrefab, _healthUIParent);
                obj.gameObject.SetActive(false);
                obj.OnHide += ReturnHealthUI;
                _healthUIPool.Enqueue(obj);
            }
        }

        public HealthUI GetHealthUI()
        {
            if (_healthUIPool.Count == 0)
            {
                CreateHealthUIPool(10);
            }
            var obj = _healthUIPool.Dequeue();
            obj.gameObject.SetActive(true);
            obj.transform.SetParent(Canvas.transform);
            return obj;
        }

        public void ReturnHealthUI(HealthUI obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_healthUIParent);
            _healthUIPool.Enqueue(obj);
        }

        private void CreateTamingUIPool(int count)
        {
            if (_tamingUIPrefab == null)
            {
                _tamingUIPrefab = GameManager.Instance.ResourceManager.LoadPrefab<TamingUI>("UI/TamingUI");
            }

            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(_tamingUIPrefab, _tamingUIParent);
                obj.gameObject.SetActive(false);
                obj.OnHide += ReturnTamingUI;
                _tamingUIPool.Enqueue(obj);
            }
        }

        public TamingUI GetTamingUI()
        {
            if (_tamingUIPool.Count == 0)
            {
                CreateTamingUIPool(10);
            }
            var obj = _tamingUIPool.Dequeue();
            obj.gameObject.SetActive(true);
            obj.transform.SetParent(Canvas.transform);
            return obj;
        }

        public void ReturnTamingUI(TamingUI obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_tamingUIParent);
            _tamingUIPool.Enqueue(obj);
        }
    }
}

