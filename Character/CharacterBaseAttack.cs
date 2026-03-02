using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OnePercent
{
    public enum ATTACK_TYPE
    {
        NORAML,
        PROJECTILE,
        AREA,
    }

    public enum CHARACTER_ATTACK_MOVE_TYPE
    {
        CHASE,
        HIT_AND_RUN,
    }

    [System.Serializable]
    public class AttackPattern
    {
        public ATTACK_TYPE AttackType;
        public int Damage;
        public float Range;
        [Range(0f, 1f), Tooltip("이 패턴이 선택될 확률 (0~1). 모든 패턴 확률 합이 1이 되도록 설정")]
        public float Probability = 0.5f;
        [Tooltip("이 패턴의 이동 방식. CHASE=쫒아가서 공격, HIT_AND_RUN=때린 후 일정 거리 도망")]
        public CHARACTER_ATTACK_MOVE_TYPE MoveType = CHARACTER_ATTACK_MOVE_TYPE.CHASE;
        [Tooltip("HIT_AND_RUN일 때 플레이어와 이 거리만큼 벌어진 뒤 멈춤")]
        public float RetreatDistance = 5f;
        [Tooltip("PROJECTILE일 때 사용할 투사체 프리팹 경로. 비어 있으면 해당 공격 스킵")]
        public string ProjectilePrefabPath;
        [Tooltip("AREA일 때 사용할 범위 공격 프리팹 경로. 비어 있으면 해당 공격 스킵")]
        public string AreaAttackPrefabPath;
    }

    public partial class CharacterBase
    {
        [SerializeField]
        private List<AttackPattern> _attackPatterns = new List<AttackPattern>();

        /// <summary>
        /// 사거리 체크용. 패턴 중 최대 사거리 반환. 패턴 없으면 0.
        /// </summary>
        public float AttackRange
        {
            get
            {
                if (_attackPatterns == null || _attackPatterns.Count == 0)
                    return 0f;
                float max = _attackPatterns[0].Range;
                for (int i = 1; i < _attackPatterns.Count; i++)
                {
                    if (_attackPatterns[i].Range > max)
                        max = _attackPatterns[i].Range;
                }
                return max;
            }
        }

        /// <summary>
        /// 기존 API 호환용. 첫 번째 패턴의 데미지 반환. 패턴 없으면 0.
        /// </summary>
        public int AttackDamage
        {
            get
            {
                if (_attackPatterns == null || _attackPatterns.Count == 0)
                    return 0;
                return _attackPatterns[0].Damage;
            }
        }

        [SerializeField]
        protected float _attackDelay = 1.5f;

        protected float _currentAttackDelay;
        private bool _isRetreating;
        private float _retreatDistance;

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

        private Dictionary<string, Queue<AttackObject>> _attackObjectPool = new Dictionary<string, Queue<AttackObject>>();
        [SerializeField]
        private Transform _projectilePoolParent;
        [SerializeField, Tooltip("AREA 공격 유지 시간(초)")]
        private float _areaAttackDuration = 3f;
        [SerializeField, Tooltip("AREA 공격 데미지 주기(초)")]
        private float _areaAttackDamageInterval = 0.5f;

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
            _isRetreating = false;
            EndAttackProcessCo();
            _currentAttackDelay = 0f;
        }

        /// <summary>
        /// 확률에 따라 공격 패턴 하나를 선택해 반환. 패턴이 없으면 null.
        /// </summary>
        protected AttackPattern GetRandomAttackPattern()
        {
            if (_attackPatterns == null || _attackPatterns.Count == 0)
                return null;

            float sum = 0f;
            for (int i = 0; i < _attackPatterns.Count; i++)
                sum += _attackPatterns[i].Probability;

            if (sum <= 0f)
                return _attackPatterns[0];

            float r = Random.Range(0f, sum);
            for (int i = 0; i < _attackPatterns.Count; i++)
            {
                r -= _attackPatterns[i].Probability;
                if (r <= 0f)
                    return _attackPatterns[i];
            }

            return _attackPatterns[_attackPatterns.Count - 1];
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

            _chaseTargetCo = ChaseTargetCo();
            StartCoroutine(_chaseTargetCo);
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
                if (CurrentAttackTarget == null || CurrentAttackTarget.IsDieCheck() || !IsEnemyCheck(CurrentAttackTarget))
                {
                    EndAttackProcessCo();
                    UpdateAttackTarget();
                    yield break;
                }

                var distance = Vector2.Distance(transform.position, CurrentAttackTarget.transform.position);

                if (_isRetreating)
                {
                    Vector2 away = (Vector2)(transform.position - CurrentAttackTarget.transform.position);
                    if (away.sqrMagnitude > 0.001f)
                        SetMoveVec(away.normalized);
                    if (distance >= _retreatDistance)
                    {
                        _isRetreating = false;
                        if (CharacterType != CHARACTER_TYPE.PLAYER)
                            SetMoveVec(Vector2.zero);
                    }
                    yield return null;
                    continue;
                }

                if (distance > AttackRange)
                {
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

                    var pattern = GetRandomAttackPattern();
                    if (pattern != null)
                    {
                        var dist = Vector2.Distance(transform.position, CurrentAttackTarget.transform.position);
                        if (dist <= pattern.Range)
                        {
                            switch (pattern.AttackType)
                            {
                                case ATTACK_TYPE.NORAML:
                                    GameManager.CombatManager.DamageCalculation(this, CurrentAttackTarget, pattern.Damage);
                                    break;
                                case ATTACK_TYPE.PROJECTILE:
                                    if (string.IsNullOrEmpty(pattern.ProjectilePrefabPath))
                                        break;
                                    var projectile = (ProjectileObject)GetPooledAttackObject<ProjectileObject>(pattern.ProjectilePrefabPath);
                                    projectile.transform.position = transform.position;
                                    projectile.Shoot(this, CurrentAttackTarget, pattern.Damage);
                                    projectile.OnReturnToPool = (p) => ReturnPooledAttackObject(p);
                                    projectile.gameObject.SetActive(true);
                                    break;
                                case ATTACK_TYPE.AREA:
                                    if (string.IsNullOrEmpty(pattern.AreaAttackPrefabPath))
                                        break;
                                    var areaAttack = (AreaAttackObject)GetPooledAttackObject<AreaAttackObject>(pattern.AreaAttackPrefabPath);
                                    areaAttack.transform.position = CurrentAttackTarget.transform.position;
                                    areaAttack.Init(this);
                                    areaAttack.OnReturnToPool = (p) => ReturnPooledAttackObject(p);
                                    areaAttack.gameObject.SetActive(true);
                                    break;

                            }

                            if (pattern.MoveType == CHARACTER_ATTACK_MOVE_TYPE.HIT_AND_RUN && CharacterType != CHARACTER_TYPE.PLAYER)
                            {
                                _isRetreating = true;
                                _retreatDistance = pattern.RetreatDistance;
                            }
                        }
                    }

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
            _isRetreating = false;
            CharacterStateChange(CHARACTER_STATE.IDLE);
            if (CharacterType != CHARACTER_TYPE.PLAYER)
                SetMoveVec(Vector2.zero);
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

        private void CreateAttackObjectPool<T>(string key, string path, int createCount) where T : AttackObject
        {
            if (!_attackObjectPool.ContainsKey(key))
            {
                _attackObjectPool[key] = new Queue<AttackObject>();
            }

            var prefab = GameManager.Instance.ResourceManager.LoadPrefab<T>(path);
            for (int i = 0; i < createCount; i++)
            {
                var parent = _projectilePoolParent != null ? _projectilePoolParent : transform;
                var obj = Instantiate(prefab, parent);
                obj.gameObject.SetActive(false);
                _attackObjectPool[key].Enqueue(obj);
            }
        }

        private AttackObject GetPooledAttackObject<T>(string path) where T : AttackObject
        {
            string key = path;
            if (!_attackObjectPool.ContainsKey(key))
            {
                CreateAttackObjectPool<T>(key, path, 10);
            }

            if (_attackObjectPool[key].Count == 0)
            {
                CreateAttackObjectPool<T>(key, path, 10);
            }

            var obj = _attackObjectPool[key].Dequeue();
            obj.PoolKey = key;
            obj.gameObject.SetActive(true);
            return obj;
        }

        private void ReturnPooledAttackObject(AttackObject obj)
        {
            string key = obj.PoolKey;
            if (string.IsNullOrEmpty(key) || !_attackObjectPool.ContainsKey(key))
                return;

            obj.OnReturnToPool = null;
            obj.gameObject.SetActive(false);
            obj.transform.position = Vector3.zero;
            obj.transform.SetParent(_projectilePoolParent != null ? _projectilePoolParent : transform);
            _attackObjectPool[key].Enqueue(obj);
        }
    }
}
