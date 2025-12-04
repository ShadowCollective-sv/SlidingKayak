using HauntedHouses.GameplaySystems.Enemies.Enemy1;
using HauntedHouses.GameplaySystems.Enemies.EnemyManager.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace HauntedHouses.GameplaySystems.Enemies.Manager
{
    public class SceneEnemyManager : MonoBehaviour
    {
        [Header("Spawn Tools")]
        public SpawnIntroducePairData[] spawnPairs = new SpawnIntroducePairData[4];
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float navMeshHitRadius = 1f;
        
        
        [HorizontalGroup("Split", MarginRight = 0.4f)]
        [Button(ButtonSizes.Large), GUIColor(0.8f, 0.8f, 0.8f)]
        private void EnemySpawner()
        {
            EnemySpawn(Random.Range(0, spawnPairs.Length));
        }

        void EnemySpawn(int pairIndex)
        {
            Vector3 spawnPosition = spawnPairs[pairIndex].spawnPoint.position;
            
            NavMeshHit hit;

            if (NavMesh.SamplePosition(spawnPosition, out hit, navMeshHitRadius, NavMesh.AllAreas))
            {
                var enemy = Instantiate(enemyPrefab, 
                    hit.position, 
                    Quaternion.identity);
                
                Debug.Log("позиция спавна " + spawnPosition);
                Debug.Log("хит у нас " + hit.position);
                // var agent = enemy.GetComponent<NavMeshAgent>();
                // if (agent != null && !agent.isOnNavMesh)
                // {
                //     agent.Warp(hit.position);
                // }
                
                var ai = enemy.GetComponent<EnemySwitchAI>();
                //Нам не нужно синглтон делать, потому что данные передаем используя this
                ai.Initialize(this, pairIndex);
                
                print("Успешно заспавнен враг на: " + hit.position);
            }

            else
            {
                Debug.LogError($"[EnemyManager] Не удалось найти валидную позицию на NavMesh " +
                               $"возле {spawnPosition} (радиус 1.5). Проверьте запекание сетки!");
            }
            
        }
    }
}

