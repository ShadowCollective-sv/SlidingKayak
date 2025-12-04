using UnityEngine;

namespace HauntedHouses.GameplaySystems.Enemies.Spawn_Gizmo_Markers
{
    public class IntroducePointGizmo : MonoBehaviour
    {
            private void OnDrawGizmos()
            {
                Gizmos.color = new Color(1f, 0.57f, 0.15f);
                Gizmos.DrawSphere(this.transform.position, 2.5f);
            
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(transform.position, Vector3.one * 1.2f);
            }
    }
}