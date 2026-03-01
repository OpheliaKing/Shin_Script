
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace OnePercent
{
    public class CpuUnit : CharacterBase
    {
        #region  군집

        [Header("군집(따라가기)")]
        [SerializeField]
        protected float _followDistance = 1.5f;

        [SerializeField]
        protected float _followSpeedMultiplier = 1f;

        [Tooltip("타겟이 이 거리 이상 떨어졌을 때만 이동 (0이면 항상 이동)")]
        [SerializeField]
        protected float _followTriggerDistance = 0.5f;

        [Tooltip("군집 유닛끼리 유지할 최소 거리")]
        [SerializeField]
        protected float _flockSeparationDistance = 0.8f;

        [Tooltip("군집 끼리 밀어내는 강도")]
        [SerializeField]
        protected float _flockSeparationStrength = 2f;

        [Tooltip("타겟과 이 거리 안이면 충돌로 간주")]
        [SerializeField]
        protected float _targetCollisionRadius = 0.5f;

        [Tooltip("타겟 진행 방향 앞에 있을 때 옆으로 비켜나는 속도 배율")]
        [SerializeField]
        protected float _sideStepSpeedMultiplier = 1.5f;


        protected Vector3 _targetLastPosition;

        protected Vector2 _lastTargetMoveDir = Vector2.down;



        #endregion

        #region  테이밍

        [Header("테이밍")]
        [SerializeField]
        private float _tamingPercent = 0.5f;

        #endregion

        private IEnumerator _dieHideCo;

        protected override void Update()
        {
            base.Update();

            if (IsDieCheck())
            {
                return;
            }

            if (_followTarget != null)
            {
                UpdateFollow();
            }
        }

        #region 군집

        public void SetFollowTarget(CharacterBase followTarget)
        {
            _followTarget = followTarget;
            _targetPositionInitialized = false;
        }

        private void UpdateFollow()
        {
            Vector3 targetPos = _followTarget.transform.position;

            if (!_targetPositionInitialized)
            {
                _targetLastPosition = targetPos;
                _targetPositionInitialized = true;
            }

            Vector2 targetMoveDir = (targetPos - _targetLastPosition);
            float targetMoveLen = targetMoveDir.magnitude;
            if (targetMoveLen > 0.001f)
            {
                targetMoveDir /= targetMoveLen;
                _lastTargetMoveDir = targetMoveDir;
            }
            else
                targetMoveDir = _lastTargetMoveDir;

            _targetLastPosition = targetPos;

            Vector2 myPos2 = transform.position;
            float distToTarget = Vector2.Distance(myPos2, (Vector2)targetPos);
            bool collidingWithTarget = distToTarget < _targetCollisionRadius && distToTarget > 0.001f;
            Vector2 fromTargetToMe = (myPos2 - (Vector2)targetPos).normalized;
            bool inFrontOfTarget = Vector2.Dot(fromTargetToMe, targetMoveDir) > 0.1f;

            if (collidingWithTarget && inFrontOfTarget)
            {
                Vector2 sideDir = new Vector2(targetMoveDir.y, -targetMoveDir.x);
                float sideStep = _moveSpeed * _sideStepSpeedMultiplier * Time.deltaTime;
                transform.position += new Vector3(sideDir.x, sideDir.y, 0f) * sideStep;
                return;
            }

            Vector3 desiredPos = targetPos - new Vector3(targetMoveDir.x, targetMoveDir.y, 0f) * _followDistance;
            desiredPos.z = transform.position.z;

            Vector2 separation = GetFlockSeparation();
            desiredPos += new Vector3(separation.x, separation.y, 0f);

            Vector3 toDesired = desiredPos - transform.position;
            float distance = toDesired.magnitude;
            if (distance < 0.001f)
                return;
            if (_followTriggerDistance > 0f && distance < _followTriggerDistance)
                return;

            float step = _moveSpeed * _followSpeedMultiplier * Time.deltaTime;
            if (step >= distance)
                transform.position = desiredPos;
            else
                transform.position += toDesired.normalized * step;
        }

        private Vector2 GetFlockSeparation()
        {
            if (_followTarget == null || _flockSeparationDistance <= 0f)
                return Vector2.zero;

            Vector2 myPos = transform.position;
            Vector2 separation = Vector2.zero;

            for (int i = 0; i < FollowTarget.FlockMembers.Count; i++)
            {
                CharacterBase other = FollowTarget.FlockMembers[i];
                if (other == this || other == _followTarget || other.FollowTarget != _followTarget)
                    continue;

                Vector2 otherPos = other.transform.position;
                Vector2 diff = myPos - otherPos;
                float dist = diff.magnitude;
                if (dist < 0.001f)
                    continue;
                if (dist >= _flockSeparationDistance)
                    continue;

                float strength = 1f - dist / _flockSeparationDistance;
                separation += diff.normalized * strength;
            }

            return separation * _flockSeparationStrength;
        }
        #endregion

        protected override void Die()
        {
            Debug.Log("Die Unit: " + gameObject.name);
            base.Die();

            if (CharacterType == CHARACTER_TYPE.NPC_ENEMY)
            {
                if (CheckTamingAble())
                {
                    ActiveTaming();
                }
            }

            if (_dieHideCo != null)
            {
                StopCoroutine(_dieHideCo);
                _dieHideCo = null;
            }
            _dieHideCo = DieHideCo();
            GameManager.Instance.StartCoroutine(_dieHideCo);
        }

        private IEnumerator DieHideCo()
        {
            var hideTime = 5f;
            var curHideTime = 0f;
            while (true)
            {
                curHideTime += Time.deltaTime;
                if (curHideTime >= hideTime)
                {
                    _dieHideCo = null;
                    //OnDieEndEvent?.Invoke();
                    yield break;
                }
                yield return null;
            }
        }

        private bool CheckTamingAble()
        {
            var rand = Random.Range(0f, 1f);
            if (rand < _tamingPercent)
            {
                return true;
            }
            return false;
        }

        protected void ActiveTaming()
        {
            var ui = GameManager.Instance.UIManager.GetTamingUI();
            ui.SetTarget(this, Taming);
        }

        protected void Taming()
        {
            var playerUnit = GameManager.Instance.CombatManager.PlayerUnit;
            if (playerUnit == null)
            {
                return;
            }
            Init();
            SetFollowTarget(playerUnit);
            playerUnit.AddFlockMember(this);

            ChangeCharacterType(CHARACTER_TYPE.NPC_FRIENDLY);
            OnDieEvent += () => playerUnit.RemoveFlockMember(this);
        }
    }
}
