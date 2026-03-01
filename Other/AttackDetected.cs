using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace OnePercent
{
    public class AttackDetected : MonoBehaviour
    {
        private CircleCollider2D _coll;

        public CircleCollider2D Coll
        {
            get
            {
                if (_coll == null)
                {
                    _coll = GetComponent<CircleCollider2D>();
                }
                return _coll;
            }
        }

        private CharacterBase _owner;

        public void Init(CharacterBase owner)
        {
            _owner = owner;
            if (owner.CharacterType == CHARACTER_TYPE.PLAYER)
            {
                Coll.radius = owner.AttackRange;
            }
        }

        public void CollActive(bool isActive)
        {
            Coll.enabled = isActive;
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            var target = other.GetComponent<CharacterBase>();
            if (target == null)
            {
                return;
            }

            if (_owner == null)
            {
                Debug.Log("Owner is null");
                return;
            }

            if (_owner.IsEnemyCheck(target))
            {
                _owner.AddAttackTarget(target);
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            var target = other.GetComponent<CharacterBase>();
            if (target == null)
            {
                return;
            }

            if (_owner.IsEnemyCheck(target))
            {
                _owner.RemoveAttackTarget(target);
            }
        }
    }

}
