using System.Collections;
using HauntedHouses.GameplaySystems.AbilitySystem.Commands;
using HauntedHouses.GameplaySystems.AbilitySystem.Interfaces;
using HauntedHouses.GameplaySystems.AbilitySystem.MainModuleCommand;
using HauntedHouses.GameplaySystems.AbilitySystem.Model.SO_Templates;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HauntedHouses.GameplaySystems.Player.Scripts.Controller
{
    /// <summary>
    /// PlayerController реализует IMovementController.
    /// Команды способностей могут работать через интерфейс, не зная о конкретной реализации PlayerController.
    /// Способностями может обладать Player, а может NPC
    /// </summary>
    public class PlayerController : MonoBehaviour, IMovementController
    {
        [Header("References")]
        [SerializeField] private CharacterModel characterData;
        [SerializeField] private AbilityHolder abilityHolder;
        
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runMultiplier = 1.5f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float gravity = -9.81f;
        
        [Header("Данные Coyote Time")]
        private AbilityData _jumpAbilityData;
        
        [Header("Mouse Look")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float mouseVerticalClamp = 80f;
        
        private CharacterController _characterController;
        private Vector3 _velocity;
        private float _horizontalMovement;
        private float _verticalMovement;
        private float _verticalRotation;
        private PlayerControls _playerControls;
        
        // Для отслеживания состояний
        private bool _wasGroundedLastFrame;
        private float _timeLeftGround;
        private bool _justJumped; // Флаг активного прыжка
        
        // ===== IMovementController Implementation =====
        
        public CharacterController CharacterController => _characterController;
        public CharacterModel CharacterModel => characterData;
        public Transform Transform => transform;
        public float Gravity => gravity;

        public void ApplyHorizontalForce(Vector3 force, float duration)
        {
            StartCoroutine(CoDash(force, duration));
        }

        public void ApplyVerticalForce(float force)
        {
            _velocity.y = force;
            _justJumped = true;
        }
        
        public Coroutine StartAbilityCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }
        
        public void StopAbilityCoroutine(Coroutine routine)
        {
            if (routine != null)
                StopCoroutine(routine);
        }
        
        // ===== Unity Lifecycle =====
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerControls = new PlayerControls();
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            _wasGroundedLastFrame = true;
        }
        
        private void Start()
        {
            // Получаем данные прыжка из AbilityHolder
            _jumpAbilityData = abilityHolder.GetAbilityData("Jump");
    
            if (_jumpAbilityData == null)
            {
                Debug.LogError("[PlayerController] Jump ability not found in AbilityHolder!");
            }
        }
        
        private void OnEnable()
        {
            _playerControls.Player.Jump.performed += OnJump;
            _playerControls.Player.Dash.performed += OnDash;
            _playerControls.Player.Enable();
        }
        
        private void OnDisable()
        {
            // Отписываемся и выключаем
            _playerControls.Player.Jump.performed -= OnJump;
            _playerControls.Player.Dash.performed -= OnDash;
            _playerControls.Player.Disable();
        }
        private void Update()
        {
            Movement();
            MouseLook();
            
            // Обновляем Coyote Time
            JumpCommand.UpdateGrounded(_characterController.isGrounded);
        }

        
        // ===== Movement =====
        private void Movement()
        {
            bool isGrounded = _characterController.isGrounded;
            
            // Гравитация
            if (isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
                _justJumped = false; // Сброс флага при приземлении
            }
            
            // ⭐ Отслеживание покидания земли
            if (!_wasGroundedLastFrame && isGrounded)
            {
                // Приземлились
                _timeLeftGround = 0f;
            }
            else if (_wasGroundedLastFrame && !isGrounded)
            {
                // Покинули землю
                if (_justJumped)
                {
                    // Активный прыжок - НЕ даём Coyote
                    _timeLeftGround = -999f;
                }
                else
                {
                    // Упали с края - даём Coyote
                    _timeLeftGround = Time.time;
                }
            }
            
            _wasGroundedLastFrame = isGrounded;
            
            // Input
            _horizontalMovement = Input.GetAxis("Horizontal");
            _verticalMovement = Input.GetAxis("Vertical");
            
            Vector3 moveDirection = transform.forward * _verticalMovement + 
                                    transform.right * _horizontalMovement;
            
            // Бег
            //bool isRunning = Input.GetKey(KeyCode.LeftShift); .WasPressedThisFrame()
            bool isRunning = _playerControls.Player.Run.IsPressed();
            float currentSpeed = walkSpeed * (isRunning ? runMultiplier : 1f);
            
            // Движение
            _characterController.Move(moveDirection * (currentSpeed * Time.deltaTime));
            
            // Гравитация
            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
            
            // Обновляем состояние персонажа (только если НЕ в приоритетном состоянии)
            UpdateCharacterState();
        }

        private void UpdateCharacterState()
        {
            //TODO метод растет, поэтому стоит его в отдельный класс-менеджер перекинуть
            
            // Не перезаписываем приоритетные состояния
            if (characterData.CurrentCharacterState == CharacterStates.Dashing ||
                characterData.CurrentCharacterState == CharacterStates.Attacking ||
                characterData.CurrentCharacterState == CharacterStates.UsingSpell)
            {
                return;
            }
            
            bool isGrounded = _characterController.isGrounded;
            bool isMoving = _horizontalMovement != 0 || _verticalMovement != 0;
            
            if (isGrounded)
            {
                // ✅ НА ЗЕМЛЕ
                if (_playerControls.Player.Run.IsPressed() && isMoving)
                    characterData.SetCharacterState(CharacterStates.Running);
                else if (isMoving)
                    characterData.SetCharacterState(CharacterStates.Walking);
                else
                    characterData.SetCharacterState(CharacterStates.Idle);
            }
            else
            {
                // ✅ В ВОЗДУХЕ - определяем какое именно состояние
                float timeSinceLeftGround = Time.time - _timeLeftGround;
                float coyoteTimeDuration = _jumpAbilityData != null ? _jumpAbilityData.coyoteTime : 0.1f;
                bool isCoyoteActive = timeSinceLeftGround <= coyoteTimeDuration && timeSinceLeftGround >= 0;
                
                if (_justJumped)
                {
                    // Активный прыжок (нажали кнопку)
                    characterData.SetCharacterState(CharacterStates.Jumping);
                }
                else if (isCoyoteActive)
                {
                    // Coyote Time активен (недавно упали)
                    characterData.SetCharacterState(CharacterStates.CoyoteFalling);
                }
                else
                {
                    // Просто в воздухе (Coyote истёк)
                    characterData.SetCharacterState(CharacterStates.InAir);
                }
            }
        }
        
        private void MouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            // Вертикальный поворот камеры
            _verticalRotation += -mouseY * mouseSensitivity;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -mouseVerticalClamp, mouseVerticalClamp);
            playerCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
            
            // Горизонтальный поворот персонажа
            transform.rotation *= Quaternion.Euler(0, mouseX * mouseSensitivity, 0);
        }
        
        // ===== Ability Triggers =====
        
        private void OnJump(InputAction.CallbackContext context)
        {
            // Регистрируем ввод ДО выполнения команды для jumpbuffer
            JumpCommand.RegisterJumpInput();
            abilityHolder.TriggerAbility("Jump");
            
        }
        
        private void OnDash(InputAction.CallbackContext context)
        {
            abilityHolder.TriggerAbility("Dash");
        }
        
        // ===== Coroutines =====
        
        private IEnumerator CoDash(Vector3 force, float duration)
        {
            CharacterStates previousState = characterData.CurrentCharacterState;
            characterData.SetCharacterState(CharacterStates.Dashing);
            
            float timer = 0f;
            while (timer < duration)
            {
                _characterController.Move(force * Time.deltaTime);
                timer += Time.deltaTime;
                yield return null;
            }
            
            characterData.SetCharacterState(previousState);
        }
        
    }
}
