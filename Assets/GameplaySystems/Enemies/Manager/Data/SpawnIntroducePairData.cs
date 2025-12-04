using Sirenix.OdinInspector;
using UnityEngine;

namespace HauntedHouses.GameplaySystems.Enemies.EnemyManager.Data
{
    [System.Serializable]
    public class SpawnIntroducePairData
    {
        [HorizontalGroup("Pair")] [PreviewField(50, ObjectFieldAlignment.Left)] [LabelText("Spawn")]
        public Transform spawnPoint;

        [HorizontalGroup("Pair")] [PreviewField(50, ObjectFieldAlignment.Left)] [LabelText("Introduce")]
        public Transform introducePoint;

    }
}