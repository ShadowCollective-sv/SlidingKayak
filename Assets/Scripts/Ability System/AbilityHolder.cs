using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace HauntedHouses.Scriptable_Object_Templates.Ability_System
{
    public class AbilityHolder : MonoBehaviour
    {
        //Who owns this ability
        public Character Owner;
        
        //The ability to trigger
        public BaseAbility Ability;
        
        //
        public AbilityStates CurrentAbilityState = AbilityStates.ReadyToUse;

        private Coroutine _handleAbilityUsage;
        
        public UnityEvent OnTriggerAbility;

        public enum AbilityStates
        {
            ReadyToUse = 0,
            Casting = 1,
            Cooldown = 2
        }
        
        ///<Summary>
        /// Triggers the ability
        ///<Summary>

        public void TriggerAbility()
        {
            //The ability can only be triggered if it's current state is ReadyToUse.
            if (CurrentAbilityState != AbilityStates.ReadyToUse)
                return;
            
            //If the character is not in an allowed state then we avoid triggering the ability.
            if (!CharacterIsOnAllowedState())
                return;
            
            //We start the process of triggering the ability.
            _handleAbilityUsage = StartCoroutine(CoHandleAbilityUsage());
        }

        //Checks if the character is in the proper state to trigger the ability.
        public bool CharacterIsOnAllowedState()
        {
            return Ability.allowedCharacterStates.Contains(Owner.CurrentCharacterState);
        }

        private IEnumerator CoHandleAbilityUsage()
        {
            //Sets the ability in casting state.
            CurrentAbilityState = AbilityStates.Casting;
            
            //Wait for casting time.
            yield return new WaitForSeconds(Ability.castingTime);
            
            //Triggers the actual ability behavior.
            Ability.Activate(this);
            
            //Sets the ability on cooldown state.
            CurrentAbilityState =  AbilityStates.Cooldown;
            
            //Invoking unity method
            OnTriggerAbility?.Invoke();
            
            //If has cooldown, handle it.
            if (Ability.hasCooldown) 
                StartCoroutine(CoHandleCooldown());
        }

        private IEnumerator CoHandleCooldown()
        {
            //Wait for cooldown time.
            yield return new WaitForSeconds(Ability.cooldown);
            
            //Sets ability ready to use.
            CurrentAbilityState = AbilityStates.ReadyToUse;
        }
    }
    
}