using System;
using UnityEngine;

namespace HauntedHouses
{
    public class LookAtPlayer : MonoBehaviour
    {
        public Transform player;

        private void Update()
        {
            if (player != null)
            {
                transform.LookAt(player); //Возможно стоит оптимизировать
            }
        }
    }
}
