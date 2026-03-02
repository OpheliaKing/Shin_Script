using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace OnePercent
{
    public class CombatManager : ManagerBase
    {
        private PlayerUnit _playerUnit;

        public PlayerUnit PlayerUnit
        {
            get
            {
                return _playerUnit;
            }
        }

        public void SetPlayerUnit(PlayerUnit playerUnit)
        {
            _playerUnit = playerUnit;
        }

        public void DamageCalculation(CharacterBase attackUnit, CharacterBase damagedUnit)
        {
            Debug.Log("DamageCalculation" + attackUnit.name + " -> " + damagedUnit.name);
            var damage = attackUnit.AttackDamage;
            damagedUnit.TakeDamage(damage);
        }

        public void DamageCalculation(CharacterBase attackUnit, CharacterBase damagedUnit, int damage)
        {
            Debug.Log("DamageCalculation" + attackUnit.name + " -> " + damagedUnit.name + " damage:" + damage);
            damagedUnit.TakeDamage(damage);
        }
    }
}

