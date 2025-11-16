using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using HauntedHouses.GameplaySystems.AbilitySystem.Model.SO_Templates;

namespace HauntedHouses.GameplaySystems.AbilitySystem
{
    /// <summary>
    /// VIEW - отображает состояние способности в UI.
    /// Подписывается на события AbilityStateModel и обновляет визуал.
    /// </summary>
    public class AbilityIconView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Main UI Elements")]
        [Tooltip("Основная иконка способности")]
        [SerializeField] private Image abilityIcon;
        
        [Tooltip("Фон иконки")]
        [SerializeField] private Image backgroundImage;
        
        [Tooltip("Рамка иконки")]
        [SerializeField] private Image borderImage;
        
        [Header("Cooldown Display")]
        [Tooltip("Заполнение кулдауна (Radial Fill)")]
        [SerializeField] private Image cooldownFillImage;
        
        [Tooltip("Текст с оставшимся временем")]
        [SerializeField] private TextMeshProUGUI cooldownText;
        
        [Tooltip("Overlay затемнения на кулдауне")]
        [SerializeField] private Image cooldownOverlay;
        
        [Header("Hotkey Display")]
        [Tooltip("Текст с горячей клавишей")]
        [SerializeField] private TextMeshProUGUI hotkeyText;
        
        [Header("Charges System")]
        [Tooltip("Контейнер для UI зарядов")]
        [SerializeField] private GameObject chargesContainer;
        
        [Tooltip("Текст с количеством зарядов (3/3)")]
        [SerializeField] private TextMeshProUGUI chargesText;
        
        [Tooltip("Визуальные индикаторы зарядов (точки/иконки)")]
        [SerializeField] private Image[] chargeIndicators;
        
        [Header("Locked State")]
        [Tooltip("Overlay когда способность заблокирована")]
        [SerializeField] private GameObject lockedOverlay;
        
        [Tooltip("Иконка замка")]
        [SerializeField] private Image lockIcon;
        
        [Tooltip("Текст 'Locked' или стоимость")]
        [SerializeField] private TextMeshProUGUI lockedText;
        
        [Header("Visual Effects")]
        [Tooltip("Эффект свечения когда способность готова")]
        [SerializeField] private GameObject readyGlowEffect;
        
        [Tooltip("Particle System для активации")]
        [SerializeField] private ParticleSystem activationParticles;
        
        [Tooltip("CanvasGroup для fade эффектов")]
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Tooltip")]
        [Tooltip("Ссылка на общий tooltip (опционально)")]
        //[SerializeField] private AbilityTooltip sharedTooltip;
        
        [Header("Visual Settings")]
        [SerializeField] private Color readyColor = Color.white;
        [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color castingColor = Color.yellow;
        [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        
        [Header("Animation Settings")]
        [SerializeField] private float pulseDuration = 0.15f;
        [SerializeField] private float pulseScale = 1.2f;
        [SerializeField] private AnimationCurve pulseEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Charge Indicator Settings")]
        [SerializeField] private Color chargeAvailableColor = Color.white;
        [SerializeField] private Color chargeDepletedColor = Color.gray;
        
        // Private references
        private RectTransform _rectTransform;
        private AbilityData _abilityData;
        private bool _isInitialized;
        
        // Animation state
        private Coroutine _pulseCoroutine;
        private Coroutine _glowCoroutine;
        
        // ===== UNITY LIFECYCLE =====
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            
            // Проверка обязательных компонентов
            if (abilityIcon == null)
                Debug.LogWarning($"[AbilityIconView] Ability Icon is not assigned on {gameObject.name}");
            
            if (cooldownFillImage == null)
                Debug.LogWarning($"[AbilityIconView] Cooldown Fill Image is not assigned on {gameObject.name}");
            
            // Скрываем необязательные элементы
            if (chargesContainer != null)
                chargesContainer.SetActive(false);
            
            if (lockedOverlay != null)
                lockedOverlay.SetActive(false);
            
            if (readyGlowEffect != null)
                readyGlowEffect.SetActive(false);
            
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
        }
        
        // ===== INITIALIZATION =====
        
        /// <summary>
        /// Инициализация View с данными способности
        /// </summary>
        public void Initialize(AbilityData data)
        {
            if (data == null)
            {
                Debug.LogError("[AbilityIconView] Cannot initialize with null AbilityData!");
                return;
            }
            
            _abilityData = data;
            
            // Устанавливаем иконку
            if (abilityIcon != null && data.icon != null)
            {
                abilityIcon.sprite = data.icon;
            }
            
            // Устанавливаем хоткей
            // if (hotkeyText != null)
            // {
            //     if (data.defaultHotkey != KeyCode.None)
            //     {
            //         string hotkeyString = data.defaultHotkey.ToString();
            //         // Убираем "Alpha" из названия клавиш (Alpha1 -> 1)
            //         hotkeyString = hotkeyString.Replace("Alpha", "");
            //         hotkeyText.text = hotkeyString;
            //         hotkeyText.gameObject.SetActive(true);
            //     }
            //     else
            //     {
            //         hotkeyText.gameObject.SetActive(false);
            //     }
            // }
            
            // Настраиваем UI зарядов
            if (data.useCharges)
            {
                SetupChargesUI(data.maxCharges);
            }
            
            // Настраиваем locked text
            if (lockedText != null && !data.isStartingAbility)
            {
                lockedText.text = $"{data.skillPointCost} SP";
            }
            
            _isInitialized = true;
            
            // Устанавливаем начальное состояние
            SetReady();
        }
        
        /// <summary>
        /// Настройка UI для системы зарядов
        /// </summary>
        private void SetupChargesUI(int maxCharges)
        {
            if (chargesContainer != null)
            {
                chargesContainer.SetActive(true);
            }
            
            // Настраиваем текст
            if (chargesText != null)
            {
                chargesText.text = $"{maxCharges}/{maxCharges}";
            }
            
            // Настраиваем визуальные индикаторы
            if (chargeIndicators != null && chargeIndicators.Length > 0)
            {
                for (int i = 0; i < chargeIndicators.Length; i++)
                {
                    if (chargeIndicators[i] != null)
                    {
                        // Показываем только нужное количество индикаторов
                        chargeIndicators[i].gameObject.SetActive(i < maxCharges);
                        
                        if (i < maxCharges)
                        {
                            chargeIndicators[i].color = chargeAvailableColor;
                        }
                    }
                }
            }
        }
        
        // ===== UPDATE METHODS (вызываются Model через события) =====
        
        /// <summary>
        /// Обновление прогресса кулдауна (0-1)
        /// </summary>
        public void UpdateCooldownProgress(float progress)
        {
            if (cooldownFillImage != null)
            {
                // progress: 0 = начало кулдауна, 1 = конец кулдауна
                // fillAmount: 1 = полностью заполнено, 0 = пусто
                cooldownFillImage.fillAmount = 1f - progress;
            }
        }
        
        /// <summary>
        /// Обновление текста с оставшимся временем кулдауна
        /// </summary>
        public void UpdateCooldownTime(float timeRemaining)
        {
            if (cooldownText == null) return;
            
            if (timeRemaining > 0f)
            {
                // Форматируем время
                if (timeRemaining >= 10f)
                {
                    cooldownText.text = Mathf.Ceil(timeRemaining).ToString("F0"); // "10", "11"
                }
                else if (timeRemaining >= 1f)
                {
                    cooldownText.text = timeRemaining.ToString("F1"); // "9.5", "1.2"
                }
                else
                {
                    cooldownText.text = timeRemaining.ToString("F1"); // "0.5"
                }
                
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Обновление зарядов
        /// </summary>
        public void UpdateCharges(int current, int max)
        {
            if (!_abilityData.useCharges) return;
            
            // Обновляем текст
            if (chargesText != null)
            {
                chargesText.text = $"{current}/{max}";
            }
            
            // Обновляем визуальные индикаторы
            if (chargeIndicators != null && chargeIndicators.Length > 0)
            {
                for (int i = 0; i < chargeIndicators.Length; i++)
                {
                    if (chargeIndicators[i] != null && i < max)
                    {
                        chargeIndicators[i].gameObject.SetActive(true);
                        
                        // Закрашиваем доступные заряды
                        chargeIndicators[i].color = i < current 
                            ? chargeAvailableColor 
                            : chargeDepletedColor;
                    }
                }
            }
        }
        
        // ===== STATE VISUAL METHODS =====
        
        /// <summary>
        /// Состояние: Готова к использованию
        /// </summary>
        public void SetReady()
        {
            // Цвет иконки
            if (abilityIcon != null)
                abilityIcon.color = readyColor;
            
            if (backgroundImage != null)
                backgroundImage.color = readyColor;
            
            // Убираем кулдаун overlay
            if (cooldownFillImage != null)
                cooldownFillImage.fillAmount = 0f;
            
            if (cooldownOverlay != null)
                cooldownOverlay.gameObject.SetActive(false);
            
            // Скрываем текст кулдауна
            if (cooldownText != null)
                cooldownText.gameObject.SetActive(false);
            
            // Показываем эффект готовности
            if (readyGlowEffect != null)
                readyGlowEffect.SetActive(true);
            
            // Полная видимость
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
            
            // Опционально: пульсация свечения
            if (readyGlowEffect != null)
            {
                StartGlowPulse();
            }
        }
        
        /// <summary>
        /// Состояние: Идет каст
        /// </summary>
        public void SetCasting()
        {
            // Желтоватый цвет
            if (abilityIcon != null)
                abilityIcon.color = castingColor;
            
            if (backgroundImage != null)
                backgroundImage.color = castingColor;
            
            // Убираем свечение
            if (readyGlowEffect != null)
                readyGlowEffect.SetActive(false);
            
            StopGlowPulse();
            
            // Можно добавить анимацию каста
            // например, вращение или пульсацию
        }
        
        /// <summary>
        /// Состояние: На кулдауне
        /// </summary>
        public void SetCooldown()
        {
            // Серый цвет
            if (abilityIcon != null)
                abilityIcon.color = cooldownColor;
            
            if (backgroundImage != null)
                backgroundImage.color = cooldownColor;
            
            // Показываем overlay
            if (cooldownOverlay != null)
                cooldownOverlay.gameObject.SetActive(true);
            
            // Убираем свечение
            if (readyGlowEffect != null)
                readyGlowEffect.SetActive(false);
            
            StopGlowPulse();
        }
        
        /// <summary>
        /// Состояние: Заблокирована
        /// </summary>
        public void SetLocked()
        {
            // Затемненный цвет
            if (abilityIcon != null)
                abilityIcon.color = lockedColor;
            
            if (backgroundImage != null)
                backgroundImage.color = lockedColor;
            
            // Показываем overlay блокировки
            if (lockedOverlay != null)
                lockedOverlay.SetActive(true);
            
            // Заполняем кулдаун полностью (визуально недоступна)
            if (cooldownFillImage != null)
                cooldownFillImage.fillAmount = 1f;
            
            // Убираем свечение
            if (readyGlowEffect != null)
                readyGlowEffect.SetActive(false);
            
            StopGlowPulse();
            
            // Частичная прозрачность
            if (canvasGroup != null)
                canvasGroup.alpha = 0.6f;
        }
        
        /// <summary>
        /// Разблокировка способности
        /// </summary>
        public void SetUnlocked()
        {
            // Убираем overlay
            if (lockedOverlay != null)
                lockedOverlay.SetActive(false);
            
            // Полная видимость
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
            
            // Играем анимацию разблокировки
            PlayUnlockAnimation();
            
            // Переводим в состояние Ready
            SetReady();
        }
        
        // ===== ANIMATIONS =====
        
        /// <summary>
        /// Эффект активации способности
        /// </summary>
        public void PlayActivationEffect()
        {
            // Пульсация масштаба
            if (_pulseCoroutine != null)
                StopCoroutine(_pulseCoroutine);
            
            _pulseCoroutine = StartCoroutine(ScalePulseCoroutine());
            
            // Particle System
            if (activationParticles != null)
            {
                activationParticles.Play();
            }
        }
        
        /// <summary>
        /// Корутина пульсации масштаба
        /// </summary>
        private IEnumerator ScalePulseCoroutine()
        {
            Vector3 originalScale = _rectTransform.localScale;
            float elapsed = 0f;
            
            // Увеличение
            while (elapsed < pulseDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (pulseDuration / 2f);
                float curveValue = pulseEaseCurve.Evaluate(t);
                
                _rectTransform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, curveValue);
                yield return null;
            }
            
            elapsed = 0f;
            
            // Уменьшение
            while (elapsed < pulseDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (pulseDuration / 2f);
                float curveValue = pulseEaseCurve.Evaluate(t);
                
                _rectTransform.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, curveValue);
                yield return null;
            }
            
            _rectTransform.localScale = originalScale;
            _pulseCoroutine = null;
        }
        
        /// <summary>
        /// Анимация разблокировки
        /// </summary>
        private void PlayUnlockAnimation()
        {
            // Яркая вспышка + пульсация
            StartCoroutine(UnlockFlashCoroutine());
            
            if (_pulseCoroutine != null)
                StopCoroutine(_pulseCoroutine);
            
            _pulseCoroutine = StartCoroutine(ScalePulseCoroutine());
            
            // Particle effect
            if (activationParticles != null)
            {
                activationParticles.Play();
            }
        }
        
        /// <summary>
        /// Вспышка при разблокировке
        /// </summary>
        private IEnumerator UnlockFlashCoroutine()
        {
            if (abilityIcon == null) yield break;
            
            Color originalColor = abilityIcon.color;
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Вспышка белого цвета
                abilityIcon.color = Color.Lerp(Color.white, originalColor, t);
                yield return null;
            }
            
            abilityIcon.color = originalColor;
        }
        
        /// <summary>
        /// Запустить пульсацию свечения
        /// </summary>
        private void StartGlowPulse()
        {
            if (readyGlowEffect == null) return;
            
            if (_glowCoroutine != null)
                StopCoroutine(_glowCoroutine);
            
            _glowCoroutine = StartCoroutine(GlowPulseCoroutine());
        }
        
        /// <summary>
        /// Остановить пульсацию свечения
        /// </summary>
        private void StopGlowPulse()
        {
            if (_glowCoroutine != null)
            {
                StopCoroutine(_glowCoroutine);
                _glowCoroutine = null;
            }
        }
        
        /// <summary>
        /// Корутина пульсации свечения
        /// </summary>
        private IEnumerator GlowPulseCoroutine()
        {
            if (readyGlowEffect == null) yield break;
            
            CanvasGroup glowCanvasGroup = readyGlowEffect.GetComponent<CanvasGroup>();
            if (glowCanvasGroup == null)
            {
                glowCanvasGroup = readyGlowEffect.AddComponent<CanvasGroup>();
            }
            
            float pulseSpeed = 1.5f;
            
            while (true)
            {
                // Fade in
                float elapsed = 0f;
                while (elapsed < pulseSpeed)
                {
                    elapsed += Time.deltaTime;
                    glowCanvasGroup.alpha = Mathf.Lerp(0.3f, 1f, elapsed / pulseSpeed);
                    yield return null;
                }
                
                // Fade out
                elapsed = 0f;
                while (elapsed < pulseSpeed)
                {
                    elapsed += Time.deltaTime;
                    glowCanvasGroup.alpha = Mathf.Lerp(1f, 0.3f, elapsed / pulseSpeed);
                    yield return null;
                }
            }
        }
        
        // // ===== TOOLTIP (опционально) =====
        //
        // /// <summary>
        // /// Показать tooltip при наведении мыши
        // /// </summary>
        // public void OnPointerEnter(PointerEventData eventData)
        // {
        //     if (!_isInitialized || _abilityData == null) return;
        //     
        //     if (sharedTooltip != null)
        //     {
        //         sharedTooltip.Show(_abilityData, transform.position);
        //     }
        // }
        //
        // /// <summary>
        // /// Скрыть tooltip при уходе мыши
        // /// </summary>
        // public void OnPointerExit(PointerEventData eventData)
        // {
        //     if (sharedTooltip != null)
        //     {
        //         sharedTooltip.Hide();
        //     }
        // }
        
        // ===== PUBLIC UTILITY =====
        
        /// <summary>
        /// Получить данные способности
        /// </summary>
        public AbilityData GetAbilityData()
        {
            return _abilityData;
        }
        
        /// <summary>
        /// Проверка инициализации
        /// </summary>
        public bool IsInitialized()
        {
            return _isInitialized;
        }
        
        // ===== CLEANUP =====
        
        private void OnDestroy()
        {
            // Останавливаем все корутины
            if (_pulseCoroutine != null)
                StopCoroutine(_pulseCoroutine);
            
            if (_glowCoroutine != null)
                StopCoroutine(_glowCoroutine);
        }
        
        // ===== EDITOR HELPER (для дебага) =====
        
#if UNITY_EDITOR
        [ContextMenu("Test - Play Activation Effect")]
        private void TestActivationEffect()
        {
            PlayActivationEffect();
        }
        
        [ContextMenu("Test - Set Ready")]
        private void TestSetReady()
        {
            SetReady();
        }
        
        [ContextMenu("Test - Set Cooldown")]
        private void TestSetCooldown()
        {
            SetCooldown();
            if (cooldownFillImage != null)
                cooldownFillImage.fillAmount = 0.5f;
            if (cooldownText != null)
            {
                cooldownText.text = "5.0";
                cooldownText.gameObject.SetActive(true);
            }
        }
        
        [ContextMenu("Test - Set Locked")]
        private void TestSetLocked()
        {
            SetLocked();
        }
        
        [ContextMenu("Test - Unlock Animation")]
        private void TestUnlockAnimation()
        {
            PlayUnlockAnimation();
        }
#endif
        public void OnPointerEnter(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }
    }
}