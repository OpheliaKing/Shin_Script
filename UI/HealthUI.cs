using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace OnePercent
{
    public class HealthUI : ObjectPositionUI
    {
        private Slider _slider;
        public Slider Slider
        {
            get
            {
                if (_slider == null)
                {
                    _slider = GetComponent<Slider>();
                }
                return _slider;
            }
        }

        private IEnumerator _hideCo;

        public Action<HealthUI> OnHide = null;

        [SerializeField]
        private float _hideTime = 1.5f;

        public void SetHealth(CharacterBase target, float maxHealth, float currentHealth)
        {
            Slider.maxValue = maxHealth;
            Slider.value = currentHealth;
            _target = target;

            if (_hideCo != null)
            {
                StopCoroutine(_hideCo);
                _hideCo = null;
            }
            _hideCo = HideCo();
            StartCoroutine(_hideCo);
        }

        protected override IEnumerator HideCo()
        {
            var curHideTime = 0f;
            while (true)
            {
                if (_target == null || _target.IsDieCheck() || _target.gameObject.activeSelf == false)
                {
                    OnHide?.Invoke(this);
                    yield break;
                }

                curHideTime += Time.deltaTime;
                if (curHideTime >= _hideTime)
                {
                    OnHide?.Invoke(this);
                    yield break;
                }
                yield return null;
            }
        }
    }
}

