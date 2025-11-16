using System;
using System.Collections.Generic;
using HauntedHouses.GameplaySystems.AbilitySystem.Model.SO_Templates;
using UnityEngine;

namespace HauntedHouses.GameplaySystems.AbilitySystem
{
    /// <summary>
    /// MODEL - система Skill Tree.
    /// Отслеживает разблокированные способности, доступные очки скиллов.
    /// </summary>
    public class SkillTreeModel
    {
        // ===== ДАННЫЕ =====
        
        private int _availableSkillPoints;
        private HashSet<string> _unlockedAbilities = new HashSet<string>();
        
        // ===== СОБЫТИЯ =====
        
        public event Action<int> OnSkillPointsChanged;
        public event Action<string> OnAbilityUnlocked;
        public event Action<string> OnAbilityLocked;
        
        // ===== ГЕТТЕРЫ =====
        
        public int AvailableSkillPoints => _availableSkillPoints;
        
        // ===== КОНСТРУКТОР =====
        
        public SkillTreeModel(int startingPoints = 0)
        {
            _availableSkillPoints = startingPoints;
        }
        
        // ===== МЕТОДЫ =====
        
        /// <summary>
        /// Инициализация - разблокирует стартовые способности
        /// </summary>
        public void Initialize(Dictionary<string, AbilityData> abilities)
        {
            foreach (var kvp in abilities)
            {
                if (kvp.Value.isStartingAbility)
                {
                    _unlockedAbilities.Add(kvp.Key);
                }
            }
        }
        
        /// <summary>
        /// Проверка: разблокирована ли способность?
        /// </summary>
        public bool IsAbilityUnlocked(string abilityName)
        {
            return _unlockedAbilities.Contains(abilityName);
        }
        
        /// <summary>
        /// Проверка: можно ли разблокировать способность?
        /// </summary>
        public bool CanUnlockAbility(AbilityData ability)
        {
            // Проверка очков
            if (_availableSkillPoints < ability.skillPointCost)
                return false;
            
            // Проверка зависимостей (требуемые способности)
            if (ability.requiredAbilities != null)
            {
                foreach (var requiredAbility in ability.requiredAbilities)
                {
                    if (requiredAbility == null) continue;
                    
                    if (!_unlockedAbilities.Contains(requiredAbility.abilityName))
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Попытка разблокировать способность
        /// </summary>
        public bool TryUnlockAbility(string abilityName, AbilityData abilityData)
        {
            // Уже разблокирована?
            if (_unlockedAbilities.Contains(abilityName))
            {
                Debug.LogWarning($"Ability '{abilityName}' is already unlocked!");
                return false;
            }
            
            // Можно разблокировать?
            if (!CanUnlockAbility(abilityData))
            {
                Debug.LogWarning($"Cannot unlock '{abilityName}': not enough skill points or missing requirements.");
                return false;
            }
            
            // Разблокируем
            _availableSkillPoints -= abilityData.skillPointCost;
            _unlockedAbilities.Add(abilityName);
            
            // Уведомляем
            OnSkillPointsChanged?.Invoke(_availableSkillPoints);
            OnAbilityUnlocked?.Invoke(abilityName);
            
            Debug.Log($"✅ Unlocked ability: {abilityName}. Remaining points: {_availableSkillPoints}");
            
            return true;
        }
        
        /// <summary>
        /// Заблокировать способность (для рефанда)
        /// </summary>
        public bool TryLockAbility(string abilityName, AbilityData abilityData)
        {
            if (!_unlockedAbilities.Contains(abilityName))
                return false;
            
            // Нельзя заблокировать стартовую способность
            if (abilityData.isStartingAbility)
            {
                Debug.LogWarning("Cannot lock a starting ability!");
                return false;
            }
            
            _unlockedAbilities.Remove(abilityName);
            _availableSkillPoints += abilityData.skillPointCost;
            
            OnSkillPointsChanged?.Invoke(_availableSkillPoints);
            OnAbilityLocked?.Invoke(abilityName);
            
            return true;
        }
        
        /// <summary>
        /// Добавить очки скиллов
        /// </summary>
        public void AddSkillPoints(int amount)
        {
            _availableSkillPoints += amount;
            OnSkillPointsChanged?.Invoke(_availableSkillPoints);
            
            Debug.Log($"💎 Added {amount} skill points. Total: {_availableSkillPoints}");
        }
        
        /// <summary>
        /// Потратить очки скиллов
        /// </summary>
        public bool TrySpendSkillPoints(int amount)
        {
            if (_availableSkillPoints < amount)
                return false;
            
            _availableSkillPoints -= amount;
            OnSkillPointsChanged?.Invoke(_availableSkillPoints);
            return true;
        }
        
        // ===== СОХРАНЕНИЕ/ЗАГРУЗКА =====
        
        /// <summary>
        /// Сохранить состояние
        /// </summary>
        public SaveData Save()
        {
            return new SaveData
            {
                skillPoints = _availableSkillPoints,
                unlockedAbilities = new List<string>(_unlockedAbilities)
            };
        }
        
        /// <summary>
        /// Загрузить состояние
        /// </summary>
        public void Load(SaveData data)
        {
            _availableSkillPoints = data.skillPoints;
            _unlockedAbilities = new HashSet<string>(data.unlockedAbilities);
            
            OnSkillPointsChanged?.Invoke(_availableSkillPoints);
            
            foreach (var abilityName in _unlockedAbilities)
            {
                OnAbilityUnlocked?.Invoke(abilityName);
            }
        }
        
        /// <summary>
        /// Сбросить все (для тестирования)
        /// </summary>
        public void Reset()
        {
            _availableSkillPoints = 0;
            _unlockedAbilities.Clear();
            
            OnSkillPointsChanged?.Invoke(_availableSkillPoints);
        }
        
        // ===== ВЛОЖЕННЫЙ КЛАСС ДЛЯ СОХРАНЕНИЯ =====
        
        [System.Serializable]
        public class SaveData
        {
            public int skillPoints;
            public List<string> unlockedAbilities;
        }
    }
}