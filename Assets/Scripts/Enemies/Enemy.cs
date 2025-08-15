using System;
using Unity.VisualScripting;
using UnityEngine;

class  Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData EnemyData;
    private float _enemyHealth;
    private float _enemySpeed;
    private float _enemySize;

    private void Start()
    {
        InitReset();
    }
    
    
    
    void InitReset() 
    {
        _enemyHealth = EnemyData.Health;
     //_enemySpeed;
     //_enemySize;
    }
    
}