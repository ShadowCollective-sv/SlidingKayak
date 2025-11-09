using HauntedHouses.Scriptable_Object_Templates.Ability_System;
using UnityEngine;
using UnityEngine.Rendering;

namespace HauntedHouses
{
    [CreateAssetMenu (fileName = "Ability", menuName = "ScriptableObjects/AbilitySystem/Dash")]
    public class DashAbility : BaseAbility
    {
        public float dashForce = 10f;
        public float dashDuration = 0.2f;
        
        public override void Activate(AbilityHolder holder)
        {
            var controller = holder.PlayerController;
            var characterData = holder.Owner;

            Vector3 dashDirection = controller.transform.forward;
            controller.ApplyHorizontalForce(dashDirection * dashForce, dashDuration);
            characterData.SetCharacterState(CharacterStates.Dashing);
            Debug.Log("работает даш");
        }
    
    }
}
