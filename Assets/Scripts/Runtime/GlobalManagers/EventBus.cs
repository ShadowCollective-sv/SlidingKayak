using UnityEngine;

namespace HauntedHouses.Runtime
{
    public class EventBus : MonoBehaviour
    {
        private static EventBus _instance;

        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError(
                        "EventBus не инициализирован, нужно проверить добавлен ли его компонент на Bootstrapper объект");
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogError("Существует дубликат EventBus. Нужно проверить, что он добавлен только один раз");
                Destroy(gameObject); //надо проверить это, не удалит ли он бутстраппер
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject); //нужно проверить стоит ли это дублировать
        }
        
        
        //scene
        
        
        //player
        
        
        //score
        
        
        //points
        
        
        //levels
        
        
        
        
        
        
        
        
    }
}
