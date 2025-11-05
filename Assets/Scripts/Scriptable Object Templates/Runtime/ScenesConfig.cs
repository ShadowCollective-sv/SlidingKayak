using UnityEngine;

namespace Scriptable_Object_Templates.Runtime
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Runtime/ScenesConfig", fileName = "ScenesConfig")]
    
    public class ScenesConfig : ScriptableObject
    {
        [Header("Addressable Keys for Scenes")]
        public string BootstrapKey = "Bootstrap";
        public string MainMenuKey = "MainMenu";
        public string GamePlayKey = "GamePlay";
    }
}