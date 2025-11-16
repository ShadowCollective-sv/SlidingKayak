using UnityEngine;
using HauntedHouses.GameplaySystems.AbilitySystem.Interfaces;
using HauntedHouses.GameplaySystems.AbilitySystem.Model.SO_Templates;

namespace HauntedHouses.GameplaySystems.AbilitySystem.Commands
{
    /// <summary>
    /// Команда прыжка с прогрессивной системой Bunny Hop.
    /// Чем больше последовательных прыжков - тем выше множитель
    /// </summary>
    public class JumpCommand : IAbilityCommand
    {
        private readonly AbilityData _data;
        
        private static float _lastJumpTime;
        private static int _bhopCombo;
        private static float _сomboMultiplierLimit = 2f;
        
        // Coyote Time + Jump Buffer
        private static float _lastGroundedTime;
        private static float _jumpInputTime;
        
        // Конструктор
        public JumpCommand(AbilityData data)
        {
            _data = data;
        }
        
        /// <summary>
        /// Сброс статических переменных при перезапуске игры (Play Mode)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _lastJumpTime = 0f;
            _bhopCombo = 0;
            _lastGroundedTime = 0f;
            _jumpInputTime = 0f;
        }
        
        public static void UpdateGrounded(bool isGrounded)
        {
            if (isGrounded)
            {
                _lastGroundedTime = Time.time;
            }
        }
        
        public static void RegisterJumpInput()
        {
            _jumpInputTime = Time.time;
        }
        
        /// <summary>
        /// Выполнение команды прыжка
        /// </summary>
        public void Execute(IMovementController controller)
        {
            float currentTime = Time.time;
            
            // === Проверка возможности прыжка ===
            // Теперь это делает CharacterStates + allowedCharacterStates
            
            float timeSinceLastJump = currentTime - _lastJumpTime;
            
            // Логика комбо Bunny Hop
            if (_data.allowBunnyHop && timeSinceLastJump > 0 && timeSinceLastJump < _data.bunnyHopTimeWindow)
            {
                if (timeSinceLastJump < _data.bunnyHopTimeWindow && timeSinceLastJump > 0)
                {
                    _bhopCombo++;
                }
                else
                {
                    _bhopCombo = 0;
                }
            }
            
            // === Множитель прыжка ===
            float comboBonus = _bhopCombo * 0.2f;
            float maxBonus = _сomboMultiplierLimit - 1f;
            float jumpMultiplier = 1f + Mathf.Min(comboBonus, maxBonus);

            // === Сила прыжка ===
            // v = sqrt(h * -2 * g), где h = jumpForce, g = gravity
            float baseVelocity = Mathf.Sqrt(_data.jumpForce * -2f * controller.Gravity);
            float finalVelocity = baseVelocity * jumpMultiplier;

            controller.ApplyVerticalForce(finalVelocity);
            
            // === Обновление состояния ===
            _lastJumpTime = currentTime;
            _jumpInputTime = 0f; // Сбрасываем буфер
            
            
            // === Логи ===
            CharacterStates state = controller.CharacterModel.CurrentCharacterState;
            string stateInfo = state == CharacterStates.CoyoteFalling ? " [Coyote]" : "";
            
            if (_bhopCombo > 0)
            {
                Debug.Log($"🐰 BUNNY HOP x{_bhopCombo}! " +
                          $"Mult: x{jumpMultiplier:F2} " +
                          $"Vel: {finalVelocity:F1}{stateInfo}");
            }
            else
            {
                Debug.Log($"⬆️ Jump! Vel: {finalVelocity:F1} " +
                          $"(Base: {baseVelocity:F1}){stateInfo}");
            }
        }
        
        /// <summary>
        /// Получить текущее комбо (для UI или дебага)
        /// </summary>
        public static int GetCurrentCombo() => _bhopCombo;
        
        /// <summary>
        /// Получить текущий множитель (для UI или дебага)
        /// </summary>
        public static float GetCurrentMultiplier()
        {
            float comboBonus = _bhopCombo * 0.2f;
            float maxBonus = _сomboMultiplierLimit - 1f;
            return 1f + Mathf.Min(comboBonus, maxBonus);
        }
        
        /// <summary>
        /// Сбросить комбо вручную (например, при смерти персонажа)
        /// </summary>
        public static void ResetCombo()
        {
            _bhopCombo = 0;
            _lastJumpTime = -999f;
            Debug.Log("🔄 Bunny Hop combo reset!");
        }
    }
}