using UnityEngine;

namespace HauntedHouses.GameplaySystems.AbilitySystem.Interfaces
{
    /// <summary>
    /// Интерфейс для объектов, которые могут получать команды от способностей.
    /// PlayerController, EnemyController, NPCController могут реализовать этот интерфейс.
    /// </summary>
    public interface IMovementController
    {
        // Компоненты
        CharacterController CharacterController { get; }
        CharacterModel CharacterModel { get; }
        Transform Transform { get; }
        
        // Физические параметры
        float Gravity { get; }
        
        // Методы для применения сил
        void ApplyHorizontalForce(Vector3 force, float duration);
        void ApplyVerticalForce(float force);
        
        // Для корутин (так как команды не MonoBehaviour)
        Coroutine StartAbilityCoroutine(System.Collections.IEnumerator routine);
        void StopAbilityCoroutine(Coroutine routine);
    }
}