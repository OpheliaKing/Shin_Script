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

        private IEnumerator _chaseTargetCo;
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
            CharacterStateChange(CHARACTER_STATE.IDLE);
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
                    if (CharacterType != CHARACTER_TYPE.PLAYER)
                    {
                        if (gameObject.activeInHierarchy)
                        {
                            _chaseTargetCo = ChaseTargetCo();
                            StartCoroutine(_chaseTargetCo);
                        }

                        return;
                    }
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

            // if (CharacterType == CHARACTER_TYPE.PLAYER)
            // {
            //     EndAttackProcessCo();
            //     _attackProcessCo = AttackProcessCo();
            //     StartCoroutine(_attackProcessCo);
            // }
            // else
            // {
            //     _chaseTargetCo = ChaseTargetCo();
            //     StartCoroutine(_chaseTargetCo);
            // }

            _chaseTargetCo = ChaseTargetCo();
            StartCoroutine(_chaseTargetCo);

            //여기서 공격 사거리 안에 있어야 공격하고 아님 이동해서 공격하는 기능 추가해야됨


            //EndAttackProcessCo();
            //_attackProcessCo = AttackProcessCo();
            //StartCoroutine(_attackProcessCo);
        }

        private IEnumerator ChaseTargetCo()
        {
            CharacterStateChange(CHARACTER_STATE.CHASE);
            while (true)
            {
                if (CurrentAttackTarget == null || CurrentAttackTarget.IsDieCheck() || !IsEnemyCheck(CurrentAttackTarget))
                {
                    EndChaseTargetCo();
                    yield break;
                }

                var distance = Vector2.Distance(transform.position, CurrentAttackTarget.transform.position);

                if (distance > AttackRange)
                {
                    //플레이어가 아닌경우 해당 캐릭터를 쫒아감
                    if (CharacterType != CHARACTER_TYPE.PLAYER)
                    {
                        SetMoveVec(CurrentAttackTarget.transform.position - transform.position);
                    }

                    yield return null;
                }
                else
                {
                    EndChaseTargetCo(false);
                    EndAttackProcessCo();
                    _attackProcessCo = AttackProcessCo();
                    StartCoroutine(_attackProcessCo);
                    yield break;
                }
            }
        }

        private void EndChaseTargetCo(bool isUpdateAttackTarget = true)
        {
            CharacterStateChange(CHARACTER_STATE.IDLE);
            if (CharacterType != CHARACTER_TYPE.PLAYER)
            {
                SetMoveVec(Vector2.zero);
            }

            if (_chaseTargetCo != null)
            {
                StopCoroutine(_chaseTargetCo);
                _chaseTargetCo = null;
            }

            if (isUpdateAttackTarget)
            {
                UpdateAttackTarget();
            }
        }

        private IEnumerator AttackProcessCo()
        {
            CharacterStateChange(CHARACTER_STATE.ATTACKING);
            while (true)
            {
                var distance = Vector2.Distance(transform.position, CurrentAttackTarget.transform.position);
                if (distance > AttackRange)
                {
                    //여기가 문제
                    // 공격 딜레이 동안 사거리를 벗어나면 어떻게 처리할지 수정해야됨
                    EndAttackProcessCo();
                    UpdateAttackTarget();
                    yield break;
                }

                if (_currentAttackDelay >= _attackDelay)
                {
                    if (CurrentAttackTarget.IsDieCheck() || !IsEnemyCheck(CurrentAttackTarget))
                    {
                        EndAttackProcessCo();
                        UpdateAttackTarget();
                        yield break;
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
            CharacterStateChange(CHARACTER_STATE.IDLE);
            if (_attackProcessCo != null)
            {
                StopCoroutine(_attackProcessCo);
                _attackProcessCo = null;
            }
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

        private void AllCoroutineStop()
        {
            if (_chaseTargetCo != null)
            {
                StopCoroutine(_chaseTargetCo);
                _chaseTargetCo = null;
            }
            if (_attackProcessCo != null)
            {
                StopCoroutine(_attackProcessCo);
                _attackProcessCo = null;
            }
        }

        protected virtual void Die()
        {
            Debug.Log($"{gameObject.name} Die");
            AllCoroutineStop();
            CharacterStateChange(CHARACTER_STATE.DEAD);
            OnDieEvent?.Invoke();
        }
    }

}
