using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BusJam.Editor
{
    // level editor: pick, add, duplicate, delete, edit levels
    [CustomEditor(typeof(LevelEditor))]
    public class LevelEditorInspector : UnityEditor.Editor
    {
        private const string LevelsFolder = "Assets/_Project/Levels";
        private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";

        private UnityEditor.Editor levelEditor;

        public override void OnInspectorGUI()
        {
            LevelEditor editor = (LevelEditor)target;

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levelDatabase"));
            serializedObject.ApplyModifiedProperties();

            LevelDatabase database = editor.LevelDatabase;
            if (database == null)
            {
                EditorGUILayout.HelpBox("Assign the Level Database.", MessageType.Warning);
                return;
            }

            DrawLevelSelection(editor, database);
            DrawLevelButtons(editor, database);

            LevelData level = editor.SelectedLevel;
            if (level == null)
                return;

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField(level.name, EditorStyles.largeLabel);

            CreateCachedEditor(level, typeof(LevelDataEditor), ref levelEditor);
            levelEditor.OnInspectorGUI();

            EditorGUILayout.Space(12);
            DrawTestButtons(editor);
        }

        private void DrawLevelSelection(LevelEditor editor, LevelDatabase database)
        {
            if (database.Count == 0)
                return;

            string[] names = new string[database.Count];
            for (int i = 0; i < database.Count; i++)
                names[i] = $"{i + 1}. {database.Levels[i].name}";

            EditorGUI.BeginChangeCheck();
            int selected = EditorGUILayout.Popup("Level", editor.SelectedIndex, names);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editor, "Select Level");
                editor.SelectedIndex = selected;
            }
        }

        private void DrawLevelButtons(LevelEditor editor, LevelDatabase database)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("New Level"))
                AddLevel(editor, database, CreateInstance<LevelData>());

            GUI.enabled = editor.SelectedLevel != null;
            if (GUILayout.Button("Duplicate"))
                AddLevel(editor, database, Instantiate(editor.SelectedLevel));

            if (GUILayout.Button("Delete"))
                DeleteSelectedLevel(editor, database);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = editor.SelectedIndex > 0;
            if (GUILayout.Button("Move Earlier"))
                MoveSelectedLevel(editor, database, -1);
            GUI.enabled = editor.SelectedIndex < database.Count - 1;
            if (GUILayout.Button("Move Later"))
                MoveSelectedLevel(editor, database, 1);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private static void AddLevel(LevelEditor editor, LevelDatabase database, LevelData level)
        {
            string path = AssetDatabase.GenerateUniqueAssetPath($"{LevelsFolder}/Level_{database.Count + 1:00}.asset");
            AssetDatabase.CreateAsset(level, path);

            Undo.RecordObject(database, "Add Level");
            database.Levels.Add(level);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            editor.SelectedIndex = database.Count - 1;
        }

        private static void DeleteSelectedLevel(LevelEditor editor, LevelDatabase database)
        {
            LevelData level = editor.SelectedLevel;
            if (!EditorUtility.DisplayDialog("Delete Level", $"Delete {level.name}?", "Delete", "Cancel"))
                return;

            database.Levels.Remove(level);
            EditorUtility.SetDirty(database);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(level));
            AssetDatabase.SaveAssets();

            editor.SelectedIndex = Mathf.Max(0, editor.SelectedIndex - 1);
        }

        private static void MoveSelectedLevel(LevelEditor editor, LevelDatabase database, int direction)
        {
            int from = editor.SelectedIndex;
            int to = from + direction;

            Undo.RecordObject(database, "Move Level");
            LevelData level = database.Levels[from];
            database.Levels.RemoveAt(from);
            database.Levels.Insert(to, level);
            EditorUtility.SetDirty(database);

            editor.SelectedIndex = to;
        }

        private static void DrawTestButtons(LevelEditor editor)
        {
            EditorGUILayout.LabelField("Testing", EditorStyles.boldLabel);

            if (GUILayout.Button("Play This Level", GUILayout.Height(28)))
            {
                AssetDatabase.SaveAssets();
                SaveSystem.LevelIndex = editor.SelectedIndex;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(GameScenePath);
                    EditorApplication.isPlaying = true;
                }
            }

            if (GUILayout.Button("Reset Saved Progress"))
                SaveSystem.LevelIndex = 0;
        }

        private void OnDisable()
        {
            if (levelEditor != null)
                DestroyImmediate(levelEditor);
        }
    }
}
