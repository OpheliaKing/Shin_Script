using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;


namespace OnePercent
{
    public class PlayerUnit : CharacterBase
    {
        protected override void Update()
        {
            base.Update();
            UpdateMove();
        }

        protected override void UpdateMove()
        {
            base.UpdateMove();
            transform.position += new Vector3(_moveVec.x, _moveVec.y, 0) * _moveSpeed * Time.deltaTime;
        }
    }
}