using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;


namespace OnePercent
{
    public enum CHARACTER_TYPE
    {
        NONE,
        PLAYER,
        NPC_ENEMY,
        NPC_BOSS,
        NPC_FRIENDLY,
    }

    public enum CHARACTER_STATE
    {
        IDLE,
        MOVING,
        ATTACKING,
        DEAD,
        CHASE,
    }

    public partial class CharacterBase : MonoBehaviour
    {
        [SerializeField]
        protected CHARACTER_TYPE _characterType;
        public CHARACTER_TYPE CharacterType
        {
            get => _characterType;
        }

        [SerializeField]
        protected CHARACTER_TYPE _defaultCharacterType;

        [SerializeField]
        protected CHARACTER_STATE _characterState;
        public CHARACTER_STATE CharacterState
        {
            get => _characterState;
        }

        [SerializeField]
        protected int _health;

        public int Health
        {
            get => _health;
        }

        [SerializeField]
        protected int _maxHealth = 10;

        protected Action _onDieEvent;

        /// <summary>
        /// 유닛이 죽었을때 이벤트 => 테이밍 기능등을 호출할때 사용
        /// </summary>
        public Action OnDieEvent
        {
            get => _onDieEvent;
            set => _onDieEvent = value;
        }

        protected Action _OnDieEndEvent;
        /// <summary>
        /// 유닛이 죽고 사라질 타이밍에 발동되는 이벤트 => 몬스터가 오브젝트 풀링 수거될때 발동
        /// </summary>
        public Action OnDieEndEvent
        {
            get => _OnDieEndEvent;
            set => _OnDieEndEvent = value;
        }

        [SerializeField]
        protected float _moveSpeed = 1f;

        [SerializeField]
        protected Vector2 _moveVec;

        protected List<CharacterBase> _flockMembers = new List<CharacterBase>();
        public List<CharacterBase> FlockMembers
        {
            get => _flockMembers;
        }

        public CharacterBase FollowTarget
        {
            get => _followTarget;
            set
            {
                _followTarget = value;
                _targetPositionInitialized = false;
            }
        }

        protected bool _targetPositionInitialized;

        protected CharacterBase _followTarget;

        private GameManager _gameManager;

        protected GameManager GameManager
        {
            get
            {
                if (_gameManager == null)
                {
                    _gameManager = GameManager.Instance;
                }
                return _gameManager;
            }
        }

        public virtual void Init()
        {
            CharacterStateChange(CHARACTER_STATE.IDLE, true);
            ChangeCharacterType(_defaultCharacterType);
            AttackInit();
            HealthInit();
        }

        private void HealthInit()
        {
            _health = _maxHealth;
        }

        protected virtual void Update()
        {

        }

        public void SetMoveVec(Vector2 moveVec)
        {
            _moveVec = moveVec;
        }

        protected virtual void UpdateMove()
        {

        }

        public virtual void ChangeCharacterType(CHARACTER_TYPE characterType)
        {
            _characterType = characterType;
        }

        public void AddFlockMember(CharacterBase member)
        {
            _flockMembers.Add(member);
        }

        public void RemoveFlockMember(CharacterBase member)
        {
            _flockMembers.Remove(member);
        }

        public bool IsEnemyCheck(CharacterBase target)
        {
            switch (CharacterType)
            {
                case CHARACTER_TYPE.PLAYER:
                case CHARACTER_TYPE.NPC_FRIENDLY:
                    return target.CharacterType == CHARACTER_TYPE.NPC_ENEMY || target.CharacterType == CHARACTER_TYPE.NPC_BOSS;
                case CHARACTER_TYPE.NPC_ENEMY:
                case CHARACTER_TYPE.NPC_BOSS:
                    return target.CharacterType == CHARACTER_TYPE.PLAYER || target.CharacterType == CHARACTER_TYPE.NPC_FRIENDLY;
                default:
                    return false;
            }
        }

        public bool IsDieCheck()
        {
            if (CharacterState == CHARACTER_STATE.DEAD || Health <= 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// DIE 상태에서 다른 상태로 넘어가지 못하게 예외처리
        /// </summary>
        /// <param name="characterState"></param>
        /// <param name="compulsoryChange"></param>
        protected virtual void CharacterStateChange(CHARACTER_STATE characterState, bool compulsoryChange = false)
        {
            if (_characterState == CHARACTER_STATE.DEAD && !compulsoryChange)
            {
                return;
            }
            _characterState = characterState;
        }
    }
}

