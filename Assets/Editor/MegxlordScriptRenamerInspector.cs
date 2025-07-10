using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

[CustomEditor(typeof(MonoScript))]
public class MegxlordScriptRenamerInspector : Editor
{
    private string newName = "";
    private string errorMessage = "";
    private string fileContentPreview = "";
    private Vector2 scrollPosition = Vector2.zero;

    public override void OnInspectorGUI()
    {
        if (target is MonoScript script)
        {
            // Форма переименования
            GUILayout.Label("Rename Script", EditorStyles.boldLabel);
            newName = EditorGUILayout.TextField("New Name", newName);

            if (string.IsNullOrWhiteSpace(newName))
            {
                ShowError("Enter a valid name for the script.");
            }
            else if (newName.Contains(' '))
            {
                ShowError("Spaces are not allowed in script names.");
            }
            else if (!Regex.IsMatch(newName, @"^\w+$"))
            {
                ShowError("Invalid script name format. Use alphanumeric characters and underscores.");
            }
            else if (GUILayout.Button("Rename"))
            {
                RenameScript(script, newName);
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                errorMessage = "";
            }

            // Превью содержимого скрипта
            GUILayout.Space(10);
            GUILayout.Label("Script Content Preview", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            try
            {
                string assetPath = AssetDatabase.GetAssetPath(script);
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string filePath = Path.GetFullPath(Path.Combine(projectRoot, assetPath));

                if (File.Exists(filePath))
                {
                    // ✅ Добавлен минимальный элемент перед GetLastRect()
                    EditorGUILayout.LabelField("", GUILayout.Height(0)); // "Пустой" элемент

                    // Теперь GetLastRect() работает корректно
                    if (fileContentPreview == "" || GUILayoutUtility.GetLastRect().height > 0)
                    {
                        fileContentPreview = File.ReadAllText(filePath);
                    }

                    EditorGUILayout.SelectableLabel(fileContentPreview, EditorStyles.wordWrappedLabel, GUILayout.ExpandHeight(true));
                }
                else
                {
                    EditorGUILayout.LabelField("File not found or has been moved.");
                }
            }
            catch (System.Exception ex)
            {
                EditorGUILayout.LabelField("Error reading script content.");
                Debug.LogError($"Error reading script content: {ex.Message}");
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }

    private void RenameScript(MonoScript script, string newClassName)
    {
        try
        {
            string assetPath = AssetDatabase.GetAssetPath(script);
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".cs"))
                throw new System.Exception("Selected object is not a C# script.");

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string filePath = Path.GetFullPath(Path.Combine(projectRoot, assetPath));

            if (!File.Exists(filePath))
                throw new System.Exception($"File not found at path: {filePath}");

            var scriptClass = script.GetClass();
            if (scriptClass == null)
                throw new System.Exception("Cannot determine class name from the script.");

            string oldClassName = scriptClass.Name;

            // Читаем содержимое файла
            string fileContent = File.ReadAllText(filePath);

            // Замена имени первого класса
            Regex classRegex = new Regex(
                $@"\bclass\s+{Regex.Escape(oldClassName)}\b",
                RegexOptions.Multiline);

            Match match = classRegex.Match(fileContent);
            if (!match.Success)
                throw new System.Exception("Class declaration not found in the script.");

            fileContent = classRegex.Replace(fileContent, $"class {newClassName}", 1);
            File.WriteAllText(filePath, fileContent);

            // Переименовываем файл
            string newFileName = $"{newClassName}.cs";
            string directory = Path.GetDirectoryName(assetPath);
            string newPath = Path.Combine(directory, newFileName);

            if (AssetDatabase.LoadAssetAtPath<MonoScript>(newPath) != null)
                throw new System.Exception("A script with this name already exists.");

            AssetDatabase.RenameAsset(assetPath, newFileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Обновляем превью
            fileContentPreview = fileContent;

            Debug.Log($"Script '{oldClassName}' renamed to '{newClassName}'.");
            newName = "";
        }
        catch (System.Exception ex)
        {
            errorMessage = ex.Message;
            Debug.LogError($"SmartRename Error: {ex.Message}");
        }
        finally
        {
            Repaint(); // Обновляем инспектор
        }
    }

    private void ShowError(string message)
    {
        errorMessage = message;
        Repaint();
    }
}