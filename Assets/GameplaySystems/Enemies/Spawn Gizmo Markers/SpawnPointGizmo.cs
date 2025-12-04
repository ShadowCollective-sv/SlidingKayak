using System;
using UnityEngine;

namespace HauntedHouses.GameplaySystems.Enemies.Spawn_Gizmo_Markers
{
    public class SpawnPointGizmo : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.aquamarine;
            Gizmos.DrawSphere(this.transform.position, 2.5f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(transform.position, Vector3.one * 1.2f);
        }
    }
}