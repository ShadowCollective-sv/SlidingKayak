using System.ComponentModel;
using HauntedHouses.Scriptable_Object_Templates.Ability_System;
using UnityEngine;

namespace HauntedHouses
{
    public enum CharacterStates
    {
        Idle,
        Walking,
        Running,
        Patroling,
        Attacking,
        Jumping,
        CoyoteFalling, //используется для CoyoteTime
        InAir,
        Dead,
        WallClimbing,
        UsingSpell,
        Dashing,
        InBubbles
    }
    //вместо Jumping можно просто добавить InAir, а двойной прыжок это просто проверка и использование прыжка
    
    public class CharacterModel : MonoBehaviour
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
