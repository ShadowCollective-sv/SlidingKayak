using System.Collections;
using System.Collections.Generic;
using HauntedHouses.GameplaySystems.AbilitySystem.Interfaces;
using HauntedHouses.GameplaySystems.AbilitySystem.Model.SO_Templates;
using HauntedHouses.Scriptable_Object_Templates.Ability_System;
using UnityEngine;

namespace HauntedHouses.GameplaySystems.AbilitySystem.MainModuleCommand
{
    /// <summary>
    /// AbilityHolder - главный Controller системы способностей
    /// Связывает Model (данные) и View (UI)
    /// </summary>
    public class AbilityHolder : MonoBehaviour
    {
        [Header("Owner")]
        [SerializeField] private CharacterModel characterData;
        
        [Header("Abilities Data (Models)")]
        [SerializeField] private List<AbilityData> abilitiesData = new List<AbilityData>();
        
        [Header("UI")]
        [SerializeField] private Transform abilitiesUIContainer;
        [SerializeField] private AbilityIconView abilityIconPrefab;
        
        [Header("Skill Tree")]
        [SerializeField] private int startingSkillPoints = 0;
        
        private IMovementController _movementController;
        
        // Models (runtime состояние)
        private Dictionary<string, AbilityStateModel> _abilityStateModels = new Dictionary<string, AbilityStateModel>();
        private SkillTreeModel _skillTreeModel;
        
        // Commands (логика выполнения)
        private Dictionary<string, IAbilityCommand> _abilityCommands = new Dictionary<string, IAbilityCommand>();
        
        // Views
        private Dictionary<string, AbilityIconView> _abilityViews = new Dictionary<string, AbilityIconView>();
        
        // Hotkeys
        private Dictionary<KeyCode, string> _hotkeyMap = new Dictionary<KeyCode, string>();
        
        public IMovementController MovementController => _movementController;
        public SkillTreeModel SkillTree => _skillTreeModel;
        
        private void Awake()
        {
            _movementController = GetComponent<IMovementController>();
            
            if (_movementController == null)
            {
                Debug.LogError("IMovementController not found!");
                return;
            }
            
            InitializeSkillTree();
            InitializeAbilities();
            CreateAbilityUI();
        }
        
        private void InitializeSkillTree()
        {
            _skillTreeModel = new SkillTreeModel(startingSkillPoints);
            
            // Преобразуем List в Dictionary для SkillTree
            var abilitiesDict = new Dictionary<string, AbilityData>();
            foreach (var data in abilitiesData)
            {
                abilitiesDict[data.abilityName] = data;
            }
            
            _skillTreeModel.Initialize(abilitiesDict);
            _skillTreeModel.OnAbilityUnlocked += HandleAbilityUnlocked;
        }
        
        private void InitializeAbilities()
        {
            foreach (var abilityData in abilitiesData)
            {
                string abilityName = abilityData.abilityName;
                
                // Проверяем разблокирована ли
                bool isUnlocked = _skillTreeModel.IsAbilityUnlocked(abilityName);
                
                // Создаем STATE MODEL (runtime состояние)
                var stateModel = new AbilityStateModel(abilityName, abilityData, isUnlocked);
                _abilityStateModels[abilityName] = stateModel;
                
                // Создаем COMMAND (логика выполнения)
                IAbilityCommand command = AbilityCommandFactory.CreateCommand(abilityData);
                if (command != null)
                {
                    _abilityCommands[abilityName] = command;
                }
                
                // // Регистрируем хоткей
                // if (abilityData.defaultHotkey != KeyCode.None)
                // {
                //     _hotkeyMap[abilityData.defaultHotkey] = abilityName;
                // }
            }
        }
        
        private void CreateAbilityUI()
        {
            if (abilitiesUIContainer == null || abilityIconPrefab == null) return;
            
            foreach (var abilityData in abilitiesData)
            {
                string abilityName = abilityData.abilityName;
                
                // Создаем View
                var iconView = Instantiate(abilityIconPrefab, abilitiesUIContainer);
                iconView.Initialize(abilityData);
                iconView.name = $"Ability_{abilityName}";
                
                _abilityViews[abilityName] = iconView;
                
                // Связываем Model ↔ View
                BindModelToView(abilityName);
            }
        }
        
        private void BindModelToView(string abilityName)
        {
            if (!_abilityStateModels.ContainsKey(abilityName)) return;
            if (!_abilityViews.ContainsKey(abilityName)) return;
            
            var model = _abilityStateModels[abilityName];
            var view = _abilityViews[abilityName];
            
            // Подписываем View на события Model
            model.OnCooldownProgressChanged += view.UpdateCooldownProgress;
            model.OnCooldownTimeChanged += view.UpdateCooldownTime;
            model.OnAbilityActivated += view.PlayActivationEffect;
            model.OnChargesChanged += view.UpdateCharges;
            
            model.OnStateChanged += (state) =>
            {
                switch (state)
                {
                    case AbilityStateModel.AbilityStates.Ready:
                        view.SetReady();
                        break;
                    case AbilityStateModel.AbilityStates.Casting:
                        view.SetCasting();
                        break;
                    case AbilityStateModel.AbilityStates.Cooldown:
                        view.SetCooldown();
                        break;
                    case AbilityStateModel.AbilityStates.Locked:
                        view.SetLocked();
                        break;
                }
            };
            
            if (!model.IsUnlocked)
            {
                view.SetLocked();
            }
        }
        
        private void Update()
        {
            // Обновляем кулдауны (Model)
            foreach (var model in _abilityStateModels.Values)
            {
                model.UpdateCooldown(Time.deltaTime);
            }
            
            // Обрабатываем хоткеи
            HandleHotkeys();
            
            
        }
        
        private void HandleHotkeys()
        {
            foreach (var kvp in _hotkeyMap)
            {
                if (Input.GetKeyDown(kvp.Key))
                {
                    TriggerAbility(kvp.Value);
                }
            }
        }
        
        /// <summary>
        /// Активация способности
        /// </summary>
        public void TriggerAbility(string abilityName)
        {
            if (!_abilityStateModels.ContainsKey(abilityName))
            {
                Debug.LogWarning($"Ability '{abilityName}' not found!");
                return;
            }
            
            if (!_abilityCommands.ContainsKey(abilityName))
            {
                Debug.LogWarning($"Command for '{abilityName}' not found!");
                return;
            }
            
            var stateModel = _abilityStateModels[abilityName];
            var abilityData = abilitiesData.Find(a => a.abilityName == abilityName);
            
            // Проверки
            if (!stateModel.IsUnlocked)
            {
                Debug.Log($"{abilityName} is locked!");
                return;
            }
            
            if (!stateModel.IsReady)
            {
                Debug.Log($"{abilityName} is not ready!");
                return;
            }
            
            if (!CharacterIsInAllowedState(abilityData))
            {
                Debug.Log($"{abilityName} blocked by character state!");
                return;
            }
            
            // Запускаем
            StartCoroutine(CoHandleAbilityUsage(abilityName));
        }
        
        private bool CharacterIsInAllowedState(AbilityData data)
        {
            return data.allowedCharacterStates.Contains(characterData.CurrentCharacterState);
        }
        
        private IEnumerator CoHandleAbilityUsage(string abilityName)
        {
            var stateModel = _abilityStateModels[abilityName];
            var abilityData = abilitiesData.Find(a => a.abilityName == abilityName);
            var command = _abilityCommands[abilityName];
            
            // 1. Casting state
            stateModel.StartCasting();
            
            // 2. Casting time
            if (abilityData.castingTime > 0f)
            {
                yield return new WaitForSeconds(abilityData.castingTime);
            }
            
            // 3. ВЫПОЛНЯЕМ КОМАНДУ!
            command.Execute(_movementController);
            
            // 4. Обновляем state (кулдаун)
            stateModel.Activate();
        }
        
        // ===== PUBLIC API =====
        
        public bool TryUnlockAbility(string abilityName)
        {
            var abilityData = abilitiesData.Find(a => a.abilityName == abilityName);
            if (abilityData == null) return false;
            
            if (_skillTreeModel.TryUnlockAbility(abilityName, abilityData))
            {
                _abilityStateModels[abilityName].Unlock();
                return true;
            }
            
            return false;
        }
        
        public void AddSkillPoints(int amount)
        {
            _skillTreeModel.AddSkillPoints(amount);
        }
        
        private void HandleAbilityUnlocked(string abilityName)
        {
            if (_abilityViews.ContainsKey(abilityName))
            {
                _abilityViews[abilityName].SetUnlocked();
            }
        }
        
        private void OnDestroy()
        {
            // Отписки
            foreach (var kvp in _abilityStateModels)
            {
                var model = kvp.Value;
                var abilityName = kvp.Key;
                
                if (_abilityViews.ContainsKey(abilityName))
                {
                    var view = _abilityViews[abilityName];
                    model.OnCooldownProgressChanged -= view.UpdateCooldownProgress;
                    model.OnCooldownTimeChanged -= view.UpdateCooldownTime;
                    model.OnAbilityActivated -= view.PlayActivationEffect;
                    model.OnChargesChanged -= view.UpdateCharges;
                }
            }
            
            if (_skillTreeModel != null)
            {
                _skillTreeModel.OnAbilityUnlocked -= HandleAbilityUnlocked;
            }
        }
        
        /// <summary>
        /// Получить AbilityData по имени способности
        /// </summary>
        public AbilityData GetAbilityData(string abilityName)
        {
            return abilitiesData.Find(a => a.abilityName == abilityName);
        }

        /// <summary>
        /// Получить AbilityData по типу способности
        /// </summary>
        public AbilityData GetAbilityDataByType(AbilityType abilityType)
        {
            return abilitiesData.Find(a => a.abilityType == abilityType);
        }
        
    }
}