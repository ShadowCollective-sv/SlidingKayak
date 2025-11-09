using UnityEngine;

namespace HauntedHouses.Game_Managers.Analytics
{
    public class AnalyticsManager : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);
        }


        public void Initialize()
        {
            Debug.Log("Аналитика подключена");
        }
    }
}
