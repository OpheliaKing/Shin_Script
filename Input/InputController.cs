using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace OnePercent
{
    public class InputController : MonoBehaviour
    {
        public void SetTargetUnit(CharacterBase targetUnit)
        {
            _targetUnit = targetUnit;
        }

        [SerializeField]
        private CharacterBase _targetUnit;

        public void OnMove(InputValue value)
        {
            _targetUnit.SetMoveVec(value.Get<Vector2>());
        }
    }
}
