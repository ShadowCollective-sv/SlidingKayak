using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

namespace HauntedHouses.Scriptable_Object_Templates.Ability_System
{
    
    
    public abstract class BaseAbility : ScriptableObject
    {
        [FormerlySerializedAs("HasCooldown")] [Header("Cooldown & Casting Time")] 
        
        public bool hasCooldown;
        public float cooldown = 1f;
        public float castingTime = 0f;
        
        [Header("Allowed States")]
        
        public List<CharacterStates> allowedCharacterStates = new List<CharacterStates>() { CharacterStates.Idle };

        public virtual void OnAbilityUpdate(AbilityHolder holder){}
        public abstract void Activate(AbilityHolder holder);

    }
}
