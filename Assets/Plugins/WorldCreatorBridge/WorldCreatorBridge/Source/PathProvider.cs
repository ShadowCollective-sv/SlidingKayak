using Plugins.WorldCreatorBridge.WorldCreatorBridge.Settings;
using UnityEditor;
using UnityEngine;

namespace BtB.WC.Bridge
{
    public class PathProvider
    {
        private static DirectoryPathData _pathData;

        private static string _exportPathProcessed;

        private static bool _isLoaded = false;

        public static string GetDirectoryPath()
        {
            if (!_isLoaded)
                LoadPathData();

            if (_pathData != null)
                return _exportPathProcessed = _pathData.directoryPath.Replace("/WCDirectoryPathData.asset", "");

            Debug.LogError("DirectoryPathData asset not found or loaded!");
            return null;
        }



        private static void LoadPathData()
        {
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:DirectoryPathData");

            if (guids.Length == 0)
            {
                Debug.LogError("DirectoryPathData asset not found in the project!");
                return;
            }

            if (guids.Length > 1)
            {
                Debug.LogWarning("Multiple DirectoryPathData assets found. Using the first one.");
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            _pathData = AssetDatabase.LoadAssetAtPath<DirectoryPathData>(assetPath);

            if (_pathData != null)
            {
                _isLoaded = true;
            }
#endif
        }
    }
}