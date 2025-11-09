using System;
using HauntedHouses.Game_Managers.Analytics;
using Runtime.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HauntedHouses.Runtime
{
    public class Bootstrapper : MonoBehaviour
    {
        private AnalyticsManager _analyticsManager;
        private LevelLoadManager _levelLoadManager;
    
        [Header("Названия сцен")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    
        private void Awake()
        {
            if (!ValidateSingleton()) return;
            DontDestroyOnLoad(gameObject);
        
            //тут инициализация менеджеров

            
        }

        private void Start()
        {
            _analyticsManager = GetOrAddComponent<AnalyticsManager>();
            
            _analyticsManager.Initialize();

            //SceneManager.LoadScene(mainMenuSceneName);
            LevelLoadManager.Instance.ExpandedLoadScene(mainMenuSceneName);
        }

        private bool ValidateSingleton()
        {
            if (FindObjectsByType<Bootstrapper>(FindObjectsSortMode.None).Length > 1) //метод надо разобрать, он новый и сортировку
            {
                Destroy(gameObject);
                return false;
            }
            return true;
        }

        private T GetOrAddComponent<T>() where T : Component
        {
            var component = GetComponent<T>();
            if (component == null)
                component = gameObject.AddComponent<T>();
            return component;
        }
    
    
    }
}

//проверяем на синглтон
//инициализируем менеджеры, чтобы они были готовы к работе
//запускаем загрузку главного меню