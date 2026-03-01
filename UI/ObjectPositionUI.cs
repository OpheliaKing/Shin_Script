using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnePercent
{
    public class ObjectPositionUI : MonoBehaviour
    {
        protected RectTransform RectParent
        {
            get
            {
                var rect = Rect;
                if (rect == null || rect.parent == null) return null;
                return rect.parent as RectTransform;
            }
        }
        protected RectTransform _rect;
        protected RectTransform Rect
        {
            get
            {
                if (_rect == null)
                {
                    _rect = GetComponent<RectTransform>();
                }
                return _rect;
            }
        }
        protected Canvas _canvas;
        protected Camera _camera;

        protected Camera Camera
        {
            get
            {
                if (_camera == null)
                    _camera = GameManager.Instance != null ? GameManager.Instance.Camera : Camera.main;
                return _camera;
            }
        }

        [SerializeField]
        protected Vector3 _offset = new Vector3(0, -0.5f, 0);

        [SerializeField]
        protected CharacterBase _target;

        protected IEnumerator _hideCo;

        /// <summary>
        /// 스크린→Rect 변환용 카메라. Overlay면 null, Screen Space - Camera면 Canvas가 쓰는 카메라.
        /// </summary>
        protected Camera GetCameraForScreenToRect()
        {
            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null) return Camera;
            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;
            return _canvas.worldCamera != null ? _canvas.worldCamera : Camera;
        }

        protected virtual void LateUpdate()
        {
            if (_target == null) return;

            RectTransform parent = RectParent;
            if (parent == null) return;

            Vector3 worldPos = _target.transform.position + _offset;
            Camera worldCam = Camera;
            if (worldCam == null) return;

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(worldCam, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, GetCameraForScreenToRect(), out Vector2 localPos);
            Rect.localPosition = new Vector3(localPos.x, localPos.y, Rect.localPosition.z);
        }

        protected virtual IEnumerator HideCo()
        {
            yield break;
        }
    }
}


