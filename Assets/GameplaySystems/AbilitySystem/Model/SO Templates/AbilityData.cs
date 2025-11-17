using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HauntedHouses.GameplaySystems.AbilitySystem.Model.SO_Templates
{
    [CreateAssetMenu(fileName = "AbilityData", menuName = "ScriptableObjects/AbilitySystem/New Ability")]
    public class AbilityData : ScriptableObject
    {
        /// <summary>
        /// Базовый класс для способностей
        /// </summary>
        
        [Header("Identity")]
        public string abilityName;
        public AbilityType abilityType = AbilityType.Jump;
        public Sprite icon;
        [TextArea(3, 5)] public string abilityDescription;
        
        //[Header("Input")]
        //public KeyCode defaultHotkey = KeyCode.None;
        //не нужно, т.к. NewInputSystem
        
        [Header("Cooldown")]
        public bool hasCooldown = true;
        public float cooldown = 1f;
        public float castingTime = 0f;
        
        [Header("Charges")]
        public bool useCharges = false;
        public int maxCharges = 1;
        public float chargeRestoreTime = 5f;
        
        [Header("Allowed States")]
        public List<CharacterStates> allowedCharacterStates = new List<CharacterStates>
        {
            CharacterStates.Idle,
            CharacterStates.Walking
        };
        
        [Header("Skill Tree")]
        public bool isStartingAbility = true;
        public int skillPointCost = 1;
        public List<AbilityData> requiredAbilities;
        
        // ===== СПЕЦИФИЧНЫЕ ДАННЫЕ ДЛЯ РАЗНЫХ ТИПОВ СПОСОБНОСТЕЙ =====
        
        [Header("Jump Settings")]
        [ShowIf("@abilityType == AbilityType.Jump")]
        public float jumpForce = 0.8f;
        [ShowIf("@abilityType == AbilityType.Jump")]
        public bool allowBunnyHop = true;
        [ShowIf("@abilityType == AbilityType.Jump")]
        public float bunnyHopMultiplier = 1.5f; // x1.5 к силе прыжка
        [ShowIf("@abilityType == AbilityType.Jump")]
        public float bunnyHopTimeWindow = 0.3f; // 0.3 секунды окно
        
        [Header("Bunny Hop Combo System")]
        [Tooltip("Бонус множителя за каждый успешный прыжок в комбо")]
        [ShowIf("@abilityType == AbilityType.Jump")]
        public float comboMultiplierStep = 0.2f; // x0.2 за каждый прыжок
        [ShowIf("@abilityType == AbilityType.Jump")]
        [Tooltip("Максимальный множитель который можно достичь")]
        public float maxComboMultiplier = 2f;    // Максимум x2.0
        
        [Header("Coyote Time + Jump Buffer")]
        [ShowIf("@abilityType == AbilityType.Jump")]
        public float coyoteTime = 0.15f;
        [ShowIf("@abilityType == AbilityType.Jump")]
        public float jumpBufferWindow = 0.1f;
        
        
        
        [Header("Dash Settings")]
        [ShowIf("@abilityType == AbilityType.Dash")]
        public float dashForce = 15f;
        [ShowIf("@abilityType == AbilityType.Dash")]
        public float dashDuration = 0.2f;
        
        [Header("Fireball Settings")]
        [ShowIf("@abilityType == AbilityType.Run")]
        public GameObject fireballPrefab;
        [ShowIf("@abilityType == AbilityType.Run")]
        public float fireballSpeed = 20f;
        [ShowIf("@abilityType == AbilityType.Run")]
        public int fireballDamage = 10;
    }
    
    /// <summary>
    /// Enum для типов способностей
    /// </summary>
    public enum AbilityType
    {
        Jump,
        Dash,
        Run,
        Slide
        
    }
    
}