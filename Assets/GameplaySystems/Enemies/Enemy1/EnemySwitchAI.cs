using System;
using HauntedHouses.GameplaySystems.Enemies.Manager;
using UnityEngine;
using UnityEngine.AI;

namespace HauntedHouses.GameplaySystems.Enemies.Enemy1
{
    [System.Serializable]
    public enum AIState 
    {
        Spawn,
        Introduce,
        Chase,
        Attack
    }

    public class EnemySwitchAI : MonoBehaviour
    {
        public AIState currentAIState;

        [Header("Utility")] 
        private bool _isIntroduced = false;
        private bool _isEnteredGameArea = false;
        [SerializeField] private Transform[] entryAIAreaPoints;

        [Header("Stats")] 
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float jumpHeight = 2f;
        
        public event Action onSpawn;
        public event Action onIntroduce;
        public event Action onChase;
        public event Action onAttack;
        
        [Header("Movement AI Settings")]
        private Transform _aiPosition;
        private float _enemyJumpHeight = 5f;
        
        [Header("Movement AI Data")]
        private SceneEnemyManager _manager;
        private int _spawnIndex;
        NavMeshAgent navMeshAgent;
        

        private void Awake()
        {
            _aiPosition = transform; //может и не нужен тогда отдельный трансформ.
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        
        

        private void Start()
        {
            currentAIState =  AIState.Spawn;
        }

        private void OnEnable()
        {
            onSpawn += Spawn;
            onIntroduce += Introduce;
            onChase += Chase;
            onAttack += Attack;
        }

        private void OnDisable()
        {
            onSpawn -= Spawn;
            onIntroduce -= Introduce;
            onChase -= Chase;
            onAttack -= Attack;
        }

        private void Update()
        {
            switch (currentAIState)
            {
                case AIState.Spawn:
                    onSpawn?.Invoke();
                    break;
                case AIState.Introduce:
                    onIntroduce?.Invoke();
                    break;
                case AIState.Chase:
                    onChase?.Invoke();
                    break;
                case AIState.Attack:
                    onAttack?.Invoke();
                    break;
            }
        }

        public void Initialize(SceneEnemyManager manager, int pairIndex )
        {
            _manager = manager;
            _spawnIndex = pairIndex;
        }
        
        void Spawn()
        {
            //transform.position -= _manager.spawnPairs[_spawnIndex].introducePoint.position;
            currentAIState = AIState.Introduce;
        }

        void Introduce()
        {
            if (_isIntroduced)
                return;
            if (!_isEnteredGameArea)
                return;
            
            transform.position = _aiPosition.position + (Vector3.up * _enemyJumpHeight);
            //тут наши действия на входе.
            
            //звук смеха
            //ИИ подпрыгивает
            
            _isIntroduced = true;
            currentAIState = AIState.Chase;
        }

        void Chase()
        {
            
            Debug.Log("Я перешел в состояние погони");
            currentAIState = AIState.Chase;
        }

        void Attack()
        {
            float distanceToPlayer = 2f;
            if (distanceToPlayer > attackRange)
                currentAIState = AIState.Chase;
            
        }
        
    }
}
