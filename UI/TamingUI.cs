using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnePercent
{
    public class TamingUI : ObjectPositionUI
    {
        public Action<TamingUI> OnHide = null;
        
        private Action _onClickEvent = null;



        public void SetTarget(CharacterBase target, Action OnClickEvent = null)
        {
            _target = target;
            _onClickEvent += OnClickEvent;
            
            _hideCo = HideCo();
            StartCoroutine(_hideCo);
        }

        protected override IEnumerator HideCo()
        {
            while (true)
            {
                if (_target == null || _target.gameObject.activeSelf == false)
                {
                    OnHide?.Invoke(this);
                    yield break;
                }
                yield return null;
            }
        }

        public void OnClickTamingButton()
        {
            _onClickEvent?.Invoke();
            OnHide?.Invoke(this);

            if (_hideCo != null)
            {
                StopCoroutine(_hideCo);
                _hideCo = null;
            }
        }
    }
}
