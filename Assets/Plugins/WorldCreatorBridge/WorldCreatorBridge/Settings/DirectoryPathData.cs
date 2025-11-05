
using UnityEditor;
using UnityEngine;

namespace Plugins.WorldCreatorBridge.WorldCreatorBridge.Settings
{
    [CreateAssetMenu(menuName = "SO/WC/DirectoryPathData", fileName = "DirectoryPathData")]

    public class DirectoryPathData : ScriptableObject
    {
        public string directoryPath = "Assets/WorldCreatorBridge";
    }
    
    [CustomEditor(typeof(DirectoryPathData))]
    public class DirectoryPathDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DirectoryPathData myScript = (DirectoryPathData)target;
            if (GUILayout.Button("Refresh Plugin Paths"))
            {
                string pathToAsset = AssetDatabase.GetAssetPath(target);
                pathToAsset = pathToAsset.Replace("/Settings/WCDirectoryPathData.asset", "");
                Debug.Log("путь изменен на " + pathToAsset);
                
                myScript.directoryPath = pathToAsset;
                EditorUtility.SetDirty(myScript); // Mark the object as dirty to ensure the change is saved
            }
        }
    }   
}



//основная проблема тут, что при любой конфигурации могут сбиться пути, поэтому лучший вариант на запуске юнити
//находить их и сохранять в скриптабл объекте


//Еще и порядок вызова метода надо проверить
//я могу найти путь, но проверка может пройти в BridgeEditor на запуске юнити и пути не заработают

