#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;

[InitializeOnLoad]
public static class SceneSwitcherToolbar
{
    private struct SceneInfo
    {
        public string displayName;
        public string path;
    }

    private static List<SceneInfo> sceneInfos = new List<SceneInfo>();
    private static string[] displayNames = new string[0];
    private static int selectedIndex = 0;
    private static string lastActiveScene = "";
    private static VisualElement toolbarUI;

    private const float positionOffset = 180f; // Margin left for toolbar positioning
    private const float dropdownBoxHeight = 20f; // Height for dropdown and toggle

    private static string[] modeLabels = { "All Scenes", "Work Scenes", "Scenes in Build" };

    private enum SceneMode
    {
        All = 0,
        Work = 1,
        Build = 2
    }

    private static SceneMode currentMode
    {
        get => (SceneMode)EditorPrefs.GetInt("SceneSwitcher_Mode", (int)SceneMode.Build);
        set => EditorPrefs.SetInt("SceneSwitcher_Mode", (int)value);
    }

    static SceneSwitcherToolbar()
    {
        // Listen for when EditorBuildSettings changes (build scenes updated)
        EditorBuildSettings.sceneListChanged += RefreshSceneList;

        // Listen for project asset changes (added/removed .unity files)
        EditorApplication.projectChanged += RefreshSceneList;

        // Hook into scene change events
        EditorSceneManager.activeSceneChangedInEditMode += (prev, current) => UpdateSceneSelection();
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        RefreshSceneList();

        EditorApplication.delayCall += AddToolbarUI;
    }

    private static void AddToolbarUI()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        try
        {
            var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null) return;

            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars.Length == 0) return;

            var toolbar = toolbars[0];
            var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rootField == null) return;

            var root = rootField.GetValue(toolbar) as VisualElement;
            if (root == null) return;

            var leftContainer = root.Q("ToolbarZoneLeftAlign");
            if (leftContainer == null) return;

            // Remove old UI if it exists to prevent duplication
            if (toolbarUI != null)
            {
                leftContainer.Remove(toolbarUI);
            }

            toolbarUI = new IMGUIContainer(OnGUI);
            toolbarUI.style.marginLeft = positionOffset;

            leftContainer.Add(toolbarUI);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to add Scene Switcher toolbar UI: " + e.Message);
        }
    }

    private static void OnGUI()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (displayNames.Length == 0)
        {
            GUILayout.Label("No scenes found", GUILayout.Height(dropdownBoxHeight));
            return;
        }

        if (selectedIndex >= displayNames.Length)
            selectedIndex = 0;

        bool isPlaying = EditorApplication.isPlaying;

        GUILayout.BeginHorizontal();

        // Mode dropdown (Disabled in Play Mode)
        EditorGUI.BeginDisabledGroup(isPlaying);
        GUIStyle modePopupStyle = new GUIStyle(EditorStyles.popup)
        {
            fixedHeight = dropdownBoxHeight
        };

        int newModeIndex = EditorGUILayout.Popup((int)currentMode, modeLabels, modePopupStyle, GUILayout.Width(100), GUILayout.Height(dropdownBoxHeight));

        if (newModeIndex != (int)currentMode)
        {
            currentMode = (SceneMode)newModeIndex;
            RefreshSceneList();
        }
        EditorGUI.EndDisabledGroup();

        // Scene dropdown (Disabled in Play Mode)
        EditorGUI.BeginDisabledGroup(isPlaying);
        GUIStyle popupStyle = new GUIStyle(EditorStyles.popup)
        {
            fixedHeight = dropdownBoxHeight
        };

        int newIndex = EditorGUILayout.Popup(selectedIndex, displayNames, popupStyle, GUILayout.Width(150), GUILayout.Height(dropdownBoxHeight));

        if (newIndex != selectedIndex)
        {
            selectedIndex = newIndex;
            LoadScene();
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Refreshes the list of scenes based on current settings, triggered by events.
    /// </summary>
    private static void RefreshSceneList()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        sceneInfos = GetSceneInfos(currentMode);
        UpdateDisplayNames();
        SelectCurrentScene();

        // Refresh toolbar UI
        if (toolbarUI != null)
            toolbarUI.MarkDirtyRepaint();
    }

    /// <summary>
    /// Retrieves the list of SceneInfo objects based on mode, filtering valid scenes and warning about missing ones.
    /// </summary>
    private static List<SceneInfo> GetSceneInfos(SceneMode mode)
    {
        var infos = new List<SceneInfo>();
        string[] missingScenes = new string[0];

        if (mode == SceneMode.All || mode == SceneMode.Work)
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && path.EndsWith(".unity"))
                {
                    bool isProjectScene = path.StartsWith("Assets/");
                    if (mode == SceneMode.Work && !isProjectScene)
                        continue; // Skip read-only package scenes for Work mode

                    string name = Path.GetFileNameWithoutExtension(path);
                    infos.Add(new SceneInfo { displayName = name, path = path });
                }
            }
        }
        else // Build
        {
            var buildScenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
            foreach (var scene in buildScenes)
            {
                if (File.Exists(scene.path))
                {
                    bool isProjectScene = scene.path.StartsWith("Assets/");
                    if (!isProjectScene)
                    {
                        Debug.LogWarning($"<color=orange>Scene Switcher:</color> Build scene \"{Path.GetFileNameWithoutExtension(scene.path)}\" is in a read-only package and skipped.");
                        continue;
                    }

                    string name = Path.GetFileNameWithoutExtension(scene.path);
                    infos.Add(new SceneInfo { displayName = name, path = scene.path });
                }
                else
                {
                    string name = Path.GetFileNameWithoutExtension(scene.path);
                    missingScenes = missingScenes.Concat(new[] { name }).ToArray();
                }
            }

            if (missingScenes.Length > 0)
            {
                Debug.LogWarning(
                    $"<color=orange>Scene Switcher:</color> Ignored {missingScenes.Length} missing scene(s) still listed in Build Settings:\n" +
                    string.Join(", ", missingScenes)
                );
            }
        }

        // Sort alphabetically for better UX
        infos = infos.OrderBy(s => s.displayName).ToList();
        return infos;
    }

    private static void UpdateDisplayNames()
    {
        displayNames = sceneInfos.Select(s => s.displayName).ToArray();
    }

    /// <summary>
    /// Selects the current scene in the dropdown, adding a placeholder if not in the current mode's list.
    /// </summary>
    private static void SelectCurrentScene()
    {
        string currentPath = EditorSceneManager.GetActiveScene().path;
        if (string.IsNullOrEmpty(currentPath))
        {
            selectedIndex = 0;
            return;
        }

        string currentName = Path.GetFileNameWithoutExtension(currentPath);
        selectedIndex = sceneInfos.FindIndex(s => s.path == currentPath);

        if (selectedIndex == -1)
        {
            // Add placeholder at the beginning only for Build mode
            if (currentMode == SceneMode.Build)
            {
                string notInBuildName = currentName + " (not in build index)";
                var notInBuild = new SceneInfo { displayName = notInBuildName, path = currentPath };
                sceneInfos.Insert(0, notInBuild);
                UpdateDisplayNames();
                selectedIndex = 0;
            }
            else
            {
                // For All/Work, if not found, just select 0 (might be package or other issue)
                selectedIndex = 0;
            }
        }

        lastActiveScene = currentName;
    }

    /// <summary>
    /// Updates selection when scene changes.
    /// </summary>
    private static void UpdateSceneSelection()
    {
        string currentScene = Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path);
        if (currentScene != lastActiveScene)
        {
            lastActiveScene = currentScene;

            // Remove any previous "not in build index" label to avoid duplicates
            sceneInfos.RemoveAll(s => s.displayName.EndsWith(" (not in build index)"));
            UpdateDisplayNames();

            SelectCurrentScene();
        }
    }

    /// <summary>
    /// Loads the selected scene, checking existence and read-only status first.
    /// </summary>
    private static void LoadScene()
    {
        if (selectedIndex >= sceneInfos.Count)
        {
            Debug.LogWarning("Invalid scene index selected.");
            return;
        }

        string scenePath = sceneInfos[selectedIndex].path;
        string sceneName = sceneInfos[selectedIndex].displayName.Replace(" (not in build index)", "");

        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError("Invalid scene path for selected scene.");
            return;
        }

        bool isPackageScene = scenePath.StartsWith("Packages/");
        if (isPackageScene)
        {
            Debug.LogWarning(
                $"<color=orange>Scene Switcher:</color> Scene \"{sceneName}\" is in a read-only package and cannot be opened in the Editor.\n" +
                $"Switch to Play Mode or use a different scene."
            );
            return;
        }

        if (!File.Exists(scenePath))
        {
            Debug.LogWarning(
                $"<color=orange>Scene Switcher:</color> Scene \"{sceneName}\" could not be found or has been deleted.\n" +
                $"Please remove it from Build Settings or re-add the file."
            );
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.delayCall += AddToolbarUI;
        }
    }
}
#endif