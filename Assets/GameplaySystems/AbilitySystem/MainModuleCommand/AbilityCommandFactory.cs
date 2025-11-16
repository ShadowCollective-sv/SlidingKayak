using HauntedHouses.GameplaySystems.AbilitySystem.Commands;
using HauntedHouses.GameplaySystems.AbilitySystem.Interfaces;
using HauntedHouses.GameplaySystems.AbilitySystem.Model.SO_Templates;
using UnityEngine;

namespace HauntedHouses.Scriptable_Object_Templates.Ability_System
{
    public class AbilityCommandFactory
    {
        public static IAbilityCommand CreateCommand(AbilityData data)
        {
            switch (data.abilityType)
            {
                case AbilityType.Jump:
                    return new JumpCommand(data);
                
               // case AbilityType.Dash:
                    //return new DashCommand(data);
                
               // case AbilityType.Fireball:
                    //return new FireballCommand(data);
                
                default:
                    Debug.LogError($"Unknown ability type: {data.abilityType}");
                    return null;
            }
        }
    }
}