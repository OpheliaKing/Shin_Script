using System.Collections.Generic;
using UnityEngine;

namespace OnePercent
{
    public class AreaAttackObject : AttackObject
    {
        private CharacterBase _owner;
        [SerializeField]
        private int _damage;
        [SerializeField]
        private float _range;
        [SerializeField]
        private float _duration;
        [SerializeField]
        private float _damageInterval;
        private float _elapsedTime;
        private float _nextDamageTime;

        /// <summary>
        /// 범위 공격 시작. 생성한 캐릭터 기준으로 적에게만 데미지.
        /// </summary>
        /// <param name="owner">이 오브젝트를 생성한 캐릭터 (적 판별 기준)</param>
        /// <param name="damage">한 번에 줄 데미지</param>
        /// <param name="range">범위 반경</param>
        /// <param name="duration">유지 시간(초)</param>
        /// <param name="damageInterval">데미지를 주는 주기(초). 0이면 매 프레임.</param>
        public void Init(CharacterBase owner)
        {
            _owner = owner;
            _elapsedTime = 0f;
            _nextDamageTime = 0f;
        }

        private void Update()
        {
            if (_owner == null)
            {
                ReturnToPool();
                return;
            }

            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= _duration)
            {
                ReturnToPool();
                return;
            }

            if (_elapsedTime >= _nextDamageTime)
            {
                ApplyDamageToEnemiesInRange();
                _nextDamageTime = _elapsedTime + _damageInterval;
            }
        }

        private void ApplyDamageToEnemiesInRange()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, _range);
            for (int i = 0; i < hits.Length; i++)
            {
                var character = hits[i].GetComponent<CharacterBase>();
                if (character == null || character.IsDieCheck())
                    continue;
                if (!_owner.IsEnemyCheck(character))
                    continue;

                GameManager.Instance.CombatManager.DamageCalculation(_owner, character, _damage);
            }
        }
    }
}
