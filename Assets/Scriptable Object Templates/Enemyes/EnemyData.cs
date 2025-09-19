using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string Name;
    public float Health;
    public float Damage;
    public float Speed;

    public Vector3VariableData currentPosition;
}