using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OnePercent
{
    public partial class CharacterBase
    {
        [SerializeField]
        protected int _attackDamage;
        public int AttackDamage
        {
            get => _attackDamage;
        }

        [SerializeField]
        protected float _attackRange;

        public float AttackRange
        {
            get => _attackRange;
        }

        [SerializeField]
        protected float _attackDelay = 1.5f;

        protected float _currentAttackDelay;

        protected List<CharacterBase> _attackTargetList = new List<CharacterBase>();
        public List<CharacterBase> AttackTarget
        {
            get => _attackTargetList;
        }

        protected CharacterBase _currentAttackTarget;

        public CharacterBase CurrentAttackTarget
        {
            get => _currentAttackTarget;
        }

        private IEnumerator _attackProcessCo;

        protected AttackDetected _attackDetected;
        protected AttackDetected AttackDetected
        {
            get
            {
                if (_attackDetected == null)
                {
                    _attackDetected = GetComponentInChildren<AttackDetected>();
                }
                return _attackDetected;
            }
        }

        private HealthUI _healthUI;

        protected void AttackInit()
        {
            AttackDetected.Init(this);
            _attackTargetList.Clear();
            _currentAttackTarget = null;
            EndAttackProcessCo();
            _currentAttackDelay = 0f;
        }

        public void AddAttackTarget(CharacterBase target)
        {
            if (_attackTargetList.Contains(target))
            {
                return;
            }
            _attackTargetList.Add(target);

            Debug.Log("AddAttackTarget");

            UpdateAttackTarget();
        }

        public void RemoveAttackTarget(CharacterBase target)
        {
            if (!_attackTargetList.Contains(target))
            {
                return;
            }
            _attackTargetList.Remove(target);
            UpdateAttackTarget();
        }

        private void UpdateAttackTarget()
        {
            if (_attackTargetList.Count == 0)
            {
                _currentAttackTarget = null;
                EndAttackProcessCo();
                return;
            }

            if (CurrentAttackTarget != null)
            {
                if (CurrentAttackTarget.IsDieCheck() || !IsEnemyCheck(CurrentAttackTarget))
                {
                    //타겟 몬스터 사망시
                    if (_attackTargetList.Contains(CurrentAttackTarget))
                    {
                        _attackTargetList.Remove(CurrentAttackTarget);
                    }
                    _currentAttackTarget = null;
                    EndAttackProcessCo();
                    UpdateAttackTarget();
                    return;
                }
                else
                {
                    //타겟 몬스터 생존시
                    return;
                }
            }

            CharacterBase target = null;
            var distance = 999f;

            for (int i = 0; i < _attackTargetList.Count; i++)
            {
                if (_attackTargetList[i].IsDieCheck())
                {
                    continue;
                }
                var dist = Vector2.Distance(transform.position, _attackTargetList[i].transform.position);
                if (dist < distance)
                {
                    distance = dist;
                    target = _attackTargetList[i];
                }
                target = _attackTargetList[i];
            }

            if (target != null)
            {
                _currentAttackTarget = target;
            }
            else
            {
                return;
            }

            EndAttackProcessCo();
            _attackProcessCo = AttackProcessCo();
            StartCoroutine(_attackProcessCo);
        }

        private IEnumerator AttackProcessCo()
        {
            _currentAttackDelay = _attackDelay;
            while (true)
            {
                if (_currentAttackDelay >= _attackDelay)
                {
                    if (CurrentAttackTarget.IsDieCheck() || !IsEnemyCheck(CurrentAttackTarget))
                    {
                        EndAttackProcessCo();
                        UpdateAttackTarget();
                        break;
                    }

                    GameManager.CombatManager.DamageCalculation(this, CurrentAttackTarget);
                    _currentAttackDelay = 0f;

                    yield return null;
                }
                else
                {
                    _currentAttackDelay += Time.deltaTime;
                    yield return null;
                }
            }
        }

        private void EndAttackProcessCo()
        {
            if (_attackProcessCo != null)
            {
                StopCoroutine(_attackProcessCo);
                _attackProcessCo = null;
            }
        }

        protected virtual void Attack()
        {

        }

        public virtual void TakeDamage(int damage)
        {
            Debug.Log($"{gameObject.name} TakeDamage: {damage}");
            _health -= damage;

            if (_healthUI == null || _healthUI?.gameObject.activeSelf == false)
            {
                _healthUI = GameManager.UIManager.GetHealthUI();
            }

            _healthUI.SetHealth(this, _maxHealth, _health);
            HealthCheck();
        }

        private void HealthCheck()
        {
            if (_health <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            Debug.Log($"{gameObject.name} Die");
            _characterState = CHARACTER_STATE.DEAD;
             OnDieEvent?.Invoke();
        }
    }

}
