using System;
using HauntedHouses.GameplaySystems.AbilitySystem.Model.SO_Templates;
using UnityEngine;

namespace HauntedHouses.GameplaySystems.AbilitySystem
{
    /// <summary>
    /// MODEL - хранит runtime состояние конкретной способности.
    /// Отслеживает кулдауны, заряды, готовность, блокировку.
    /// НЕ содержит логики выполнения - только данные и их обновление.
    /// </summary>
    public class AbilityStateModel
    {
        // ===== ДАННЫЕ =====
        
        public string AbilityName { get; private set; }
        public AbilityData AbilityData { get; private set; }
        
        // Состояние
        private AbilityStates _currentState;
        private bool _isUnlocked;
        
        // Кулдаун
        private float _cooldownRemaining;
        
        // Заряды
        private int _currentCharges;
        private float _chargeRestoreTimer;
        
        // ===== СОБЫТИЯ (для View) =====
        
        public event Action<float> OnCooldownProgressChanged; // 0-1 (прогресс)
        public event Action<float> OnCooldownTimeChanged;     // секунды
        public event Action<AbilityStates> OnStateChanged;
        public event Action OnAbilityActivated;
        public event Action<int, int> OnChargesChanged;       // (current, max)
        
        // ===== ENUM =====
        
        public enum AbilityStates
        {
            Ready,      // Готова к использованию
            Casting,    // Идет каст
            Cooldown,   // На кулдауне
            Locked      // Заблокирована (не куплена)
        }
        
        // ===== ГЕТТЕРЫ =====
        
        public AbilityStates CurrentState => _currentState;
        public bool IsUnlocked => _isUnlocked;
        public float CooldownRemaining => _cooldownRemaining;
        public int CurrentCharges => _currentCharges;
        public int MaxCharges => AbilityData.useCharges ? AbilityData.maxCharges : 1;
        
        /// <summary>
        /// Способность готова к использованию?
        /// </summary>
        public bool IsReady
        {
            get
            {
                if (!_isUnlocked) return false;
                if (_currentState != AbilityStates.Ready) return false;
                
                if (AbilityData.useCharges)
                    return _currentCharges > 0;
                
                return true;
            }
        }
        
        /// <summary>
        /// Прогресс кулдауна (0-1)
        /// </summary>
        public float CooldownProgress
        {
            get
            {
                if (AbilityData.useCharges)
                {
                    // Прогресс восстановления следующего заряда
                    if (_currentCharges >= AbilityData.maxCharges)
                        return 1f;
                    
                    return Mathf.Clamp01(_chargeRestoreTimer / AbilityData.chargeRestoreTime);
                }
                else if (AbilityData.hasCooldown)
                {
                    // Обычный кулдаун
                    return Mathf.Clamp01(1f - (_cooldownRemaining / AbilityData.cooldown));
                }
                
                return 1f;
            }
        }
        
        // ===== КОНСТРУКТОР =====
        
        public AbilityStateModel(string name, AbilityData abilityData, bool unlocked = true)
        {
            AbilityName = name;
            AbilityData = abilityData;
            _isUnlocked = unlocked;
            
            // Инициализация зарядов
            if (abilityData.useCharges)
            {
                _currentCharges = abilityData.maxCharges;
            }
            else
            {
                _currentCharges = 1;
            }
            
            _currentState = unlocked ? AbilityStates.Ready : AbilityStates.Locked;
            _cooldownRemaining = 0f;
            _chargeRestoreTimer = 0f;
        }
        
        // ===== МЕТОДЫ УПРАВЛЕНИЯ СОСТОЯНИЕМ =====
        
        /// <summary>
        /// Начать каст способности
        /// </summary>
        public void StartCasting()
        {
            if (!_isUnlocked) return;
            SetState(AbilityStates.Casting);
        }
        
        /// <summary>
        /// Активировать способность (после каста)
        /// Запускает кулдаун или тратит заряд
        /// </summary>
        public void Activate()
        {
            if (!_isUnlocked) return;
            
            OnAbilityActivated?.Invoke();
            
            if (AbilityData.useCharges)
            {
                // Система зарядов
                _currentCharges = Mathf.Max(0, _currentCharges - 1);
                OnChargesChanged?.Invoke(_currentCharges, MaxCharges);
                
                // Если зарядов больше нет - переходим в кулдаун
                if (_currentCharges == 0)
                {
                    SetState(AbilityStates.Cooldown);
                    _chargeRestoreTimer = 0f;
                }
                else
                {
                    // Есть еще заряды - сразу готова снова
                    SetState(AbilityStates.Ready);
                }
            }
            else if (AbilityData.hasCooldown)
            {
                // Обычный кулдаун
                StartCooldown();
            }
            else
            {
                // Нет кулдауна - сразу готова
                SetState(AbilityStates.Ready);
            }
        }
        
        /// <summary>
        /// Запустить кулдаун
        /// </summary>
        public void StartCooldown()
        {
            _cooldownRemaining = AbilityData.cooldown;
            SetState(AbilityStates.Cooldown);
            OnCooldownTimeChanged?.Invoke(_cooldownRemaining);
            OnCooldownProgressChanged?.Invoke(0f);
        }
        
        /// <summary>
        /// Обновить кулдаун (вызывать каждый кадр)
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            if (!_isUnlocked) return;
            
            if (AbilityData.useCharges)
            {
                // Восстановление зарядов
                if (_currentCharges < AbilityData.maxCharges)
                {
                    _chargeRestoreTimer += deltaTime;
                    
                    // Уведомляем View о прогрессе
                    OnCooldownProgressChanged?.Invoke(CooldownProgress);
                    OnCooldownTimeChanged?.Invoke(AbilityData.chargeRestoreTime - _chargeRestoreTimer);
                    
                    // Заряд восстановлен?
                    if (_chargeRestoreTimer >= AbilityData.chargeRestoreTime)
                    {
                        _currentCharges++;
                        _chargeRestoreTimer = 0f;
                        
                        OnChargesChanged?.Invoke(_currentCharges, MaxCharges);
                        
                        // Если восстановили хотя бы один заряд - способность готова
                        if (_currentCharges > 0 && _currentState == AbilityStates.Cooldown)
                        {
                            SetState(AbilityStates.Ready);
                        }
                    }
                }
            }
            else if (_currentState == AbilityStates.Cooldown)
            {
                // Обычный кулдаун
                _cooldownRemaining -= deltaTime;
                
                OnCooldownTimeChanged?.Invoke(_cooldownRemaining);
                OnCooldownProgressChanged?.Invoke(CooldownProgress);
                
                if (_cooldownRemaining <= 0f)
                {
                    _cooldownRemaining = 0f;
                    SetState(AbilityStates.Ready);
                }
            }
        }
        
        /// <summary>
        /// Разблокировать способность
        /// </summary>
        public void Unlock()
        {
            _isUnlocked = true;
            SetState(AbilityStates.Ready);
        }
        
        /// <summary>
        /// Заблокировать способность
        /// </summary>
        public void Lock()
        {
            _isUnlocked = false;
            SetState(AbilityStates.Locked);
        }
        
        /// <summary>
        /// Изменить состояние
        /// </summary>
        private void SetState(AbilityStates newState)
        {
            if (_currentState == newState) return;
            
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
        }
    }
}