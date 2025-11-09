using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using VFavorites.Libs;

namespace HauntedHouses.Scriptable_Object_Templates.Ability_System
{
    // Чтобы сериализовать словарь в инспекторе: ключ = имя способности, значение = ScriptableObject способности
    [System.Serializable]
    public class AbilityDictionary : VUtils.SerializableDictionary<string,BaseAbility> {}
    
    public class AbilityHolder : MonoBehaviour
    {
        [Header("Owner")]
        //Who owns this ability
        public CharacterData Owner;
        
        [Header("Abilities")]
        [DictionaryDrawerSettings(KeyLabel = "Ability Name", ValueLabel = "Ability")]
        public AbilityDictionary Abilities = new AbilityDictionary();
        
        private PlayerController _playerController;
        public PlayerController PlayerController => _playerController;
        
        //состояния в которых находятся умения
        private Dictionary<string, AbilityStates> _abilityStates = new Dictionary<string, AbilityStates>();

        //private Coroutine _handleAbilityUsage; ссылка на корутину может пригодиться для прерывания каста
        public UnityEvent<string> OnTriggerAbility;

        public enum AbilityStates
        {
            ReadyToUse = 0,
            Casting = 1,
            Cooldown = 2
        }
        
        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            foreach (var kvp in Abilities)
            {
                _abilityStates[kvp.Key] = AbilityStates.ReadyToUse;
            }
        }
        
        /// <Summary>
        /// Запуск способности по имени
        /// <Summary>

        public void TriggerAbility(string abilityName)
        {
            if (!Abilities.ContainsKey(abilityName)) return;

            BaseAbility ability = Abilities[abilityName];
                
            //The ability can only be triggered if it's current state is ReadyToUse.
            if (_abilityStates[abilityName] != AbilityStates.ReadyToUse)
                return;
            
            //If the character is not in an allowed state then we avoid triggering the ability.
            if (!CharacterIsOnAllowedState(ability))
                return;
            
            //We start the process of triggering the ability.
            StartCoroutine(CoHandleAbilityUsage(abilityName));
        }

        //Checks if the character is in the proper state to trigger the ability.
        private bool CharacterIsOnAllowedState(BaseAbility ability)
        {
            return ability.allowedCharacterStates.Contains(Owner.CurrentCharacterState);
        }

        private IEnumerator CoHandleAbilityUsage(string abilityName)
        {
            BaseAbility ability = Abilities[abilityName];
            
            //Sets the ability in casting state.
            _abilityStates[abilityName] = AbilityStates.Casting;
            
            //Wait for casting time.
            yield return new WaitForSeconds(ability.castingTime);
            
            //Triggers the actual ability behavior.
            ability.Activate(this);
            
            //Sets the ability on cooldown state.
            _abilityStates[abilityName] =  AbilityStates.Cooldown;
            
            //Invoking unity method
            OnTriggerAbility?.Invoke(abilityName);
            
            //If has cooldown, handle it.
            if (ability.hasCooldown) 
                StartCoroutine(CoHandleCooldown(abilityName));
        }

        private IEnumerator CoHandleCooldown(string abilityName)
        {
            //Wait for cooldown time.
            yield return new WaitForSeconds(Abilities[abilityName].cooldown);
            
            //Sets ability ready to use.
            _abilityStates[abilityName] = AbilityStates.ReadyToUse;
        }
    }
    
}