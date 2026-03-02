using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnePercent
{
    public class ProjectileObject : AttackObject
    {

        private CharacterBase _owner;

        public CharacterBase Owner
        {
            get => _owner;
        }

        private CharacterBase _target;
        private Vector2 _lastMoveDir;
        [SerializeField]
        private float _speed = 1f;
        [SerializeField, Tooltip("초당 회전 속도(도). 클수록 빠르게 조준, 작을수록 부드럽게 전환")]
        private float _aimRotationSpeed = 360f;
        [SerializeField, Tooltip("스프라이트 기준 방향 보정. 오른쪽=0, 위=90, 왼쪽=180")]
        private float _forwardAngleOffset = 0f;

        [SerializeField]
        private float _lifeTime = 3f;
        private float _currentLifeTime = 0f;

        private int _damage;
        public int Damage => _damage;

        public void Shoot(CharacterBase owner, CharacterBase target, int damage)
        {
            _owner = owner;
            _target = target;
            _damage = damage;
            _currentLifeTime = 0f;
            if (target != null)
            {
                Vector2 to = (Vector2)(target.transform.position - transform.position);
                if (to.sqrMagnitude > 0.001f)
                    _lastMoveDir = to.normalized;
            }
        }

        private void Update()
        {
            Vector2 dir;
            if (_target != null && !_target.IsDieCheck())
            {
                Vector2 toTarget = (Vector2)(_target.transform.position - transform.position);
                float dist = toTarget.magnitude;
                if (dist < 0.001f)
                {
                    dir = _lastMoveDir;
                }
                else
                {
                    dir = toTarget / dist;
                    _lastMoveDir = dir;
                }
                // 머리 부분이 타겟을 향하도록 회전
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + _forwardAngleOffset;
                float currentAngle = transform.eulerAngles.z;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, _aimRotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
            }
            else
            {
                // 타겟이 사라졌으면 진행하던 방향으로 계속 이동
                dir = _lastMoveDir.sqrMagnitude > 0.001f ? _lastMoveDir : (Vector2)transform.right;
            }

            float step = _speed * Time.deltaTime;
            transform.position += (Vector3)(dir * step);

            _currentLifeTime += Time.deltaTime;
            if (_currentLifeTime > _lifeTime)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var hitCharacter = other.GetComponent<CharacterBase>();
            if (hitCharacter == null)
                return;

            if (_owner.IsEnemyCheck(hitCharacter))
            {
                OnHitTarget(hitCharacter);
            }

        }

        /// <summary>
        /// 충돌 시 호출. 데미지 처리 후 풀 반환은 base.ReturnToPool() 호출.
        /// </summary>
        protected virtual void OnHitTarget(CharacterBase hitTarget)
        {
            GameManager.Instance.CombatManager.DamageCalculation(_owner, hitTarget, _damage);
            ReturnToPool();
        }
    }
}
