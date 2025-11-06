using System.ComponentModel;
using HauntedHouses.Scriptable_Object_Templates.Ability_System;
using UnityEngine;

namespace HauntedHouses
{
    public enum CharacterStates
    {
        Idle,
        Walking,
        Patroling,
        Attacking,
        Jumping,
        Dead,
        WallClimbing,
        UsingSpell,
        Dashing,
        InBubbles
    }
    
    public class Character : MonoBehaviour
    {
        [SerializeField, ReadOnly(true)] private CharacterStates _currentCharacterState = CharacterStates.Idle;

        public CharacterStates CurrentCharacterState
        {
            get => _currentCharacterState;
            private set => _currentCharacterState = value;
        }
        
        public void SetCharacterState(CharacterStates newState) => CurrentCharacterState = newState;
        
    }
}
