using System;
using UnityEditor;
using UnityEngine;

namespace BusJam.Editor
{
    // LevelData inspector: paint the grid, set size, time and buses
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : UnityEditor.Editor
    {
        private const float CellButtonSize = 34f;

        private static CellType brushType = CellType.Passenger;
        private static GameColor brushColor = GameColor.Red;

        public override void OnInspectorGUI()
        {
            LevelData level = (LevelData)target;

            DrawSettings(level);
            EditorGUILayout.Space(10);
            DrawBrush();
            EditorGUILayout.Space(6);
            DrawGrid(level);
            EditorGUILayout.Space(10);
            DrawBuses(level);
            EditorGUILayout.Space(10);
            DrawValidation(level);
        }

        private void DrawSettings(LevelData level)
        {
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            int newWidth = EditorGUILayout.IntSlider("Width", level.Width, LevelData.MinSize, LevelData.MaxSize);
            int newHeight = EditorGUILayout.IntSlider("Height", level.Height, LevelData.MinSize, LevelData.MaxSize);
            float newTime = EditorGUILayout.FloatField("Time Limit (seconds)", level.TimeLimit);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(level, "Edit Level Settings");
                if (newWidth != level.Width || newHeight != level.Height)
                    level.Resize(newWidth, newHeight);
                level.SetTimeLimit(newTime);
                EditorUtility.SetDirty(level);
            }
        }

        private static void DrawBrush()
        {
            EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawBrushButton("Empty", CellType.Empty, GameColor.Red, Color.white);
            DrawBrushButton("Wall", CellType.Obstacle, GameColor.Red, Color.gray);
            DrawBrushButton("Hole", CellType.Hole, GameColor.Red, new Color(0.15f, 0.15f, 0.15f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            foreach (GameColor color in Enum.GetValues(typeof(GameColor)))
                DrawBrushButton(color.ToString(), CellType.Passenger, color, GameColors.ToColor(color));
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawBrushButton(string label, CellType type, GameColor color, Color buttonColor)
        {
            bool selected = brushType == type && (type != CellType.Passenger || brushColor == color);
            GUI.backgroundColor = buttonColor;
            string text = selected ? $"[{label}]" : label;
            if (GUILayout.Button(text, GUILayout.Height(24)))
            {
                brushType = type;
                brushColor = color;
            }
            GUI.backgroundColor = Color.white;
        }

        private static void DrawGrid(LevelData level)
        {
            EditorGUILayout.LabelField("Grid (click to paint, top row is next to the bus stop)", EditorStyles.boldLabel);

            for (int y = 0; y < level.Height; y++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int x = 0; x < level.Width; x++)
                {
                    CellData cell = level.GetCell(x, y);
                    GUI.backgroundColor = GetCellColor(cell);
                    string label = cell.type == CellType.Obstacle ? "X" : "";

                    if (GUILayout.Button(label, GUILayout.Width(CellButtonSize), GUILayout.Height(CellButtonSize)))
                    {
                        Undo.RecordObject(level, "Paint Cell");
                        level.SetCell(x, y, new CellData { type = brushType, color = brushColor });
                        EditorUtility.SetDirty(level);
                    }
                }

                GUI.backgroundColor = Color.white;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawBuses(LevelData level)
        {
            EditorGUILayout.LabelField("Buses (in arrival order)", EditorStyles.boldLabel);

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("busColors"), true);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Auto Fill Buses From Passengers"))
            {
                Undo.RecordObject(level, "Auto Fill Buses");
                level.AutoFillBuses();
                EditorUtility.SetDirty(level);
            }
        }

        private static void DrawValidation(LevelData level)
        {
            foreach (GameColor color in Enum.GetValues(typeof(GameColor)))
            {
                int count = level.CountPassengers(color);
                if (count > 0)
                    EditorGUILayout.LabelField($"{color} passengers", count.ToString());
            }

            bool valid = level.IsValid(out string message);
            EditorGUILayout.HelpBox(message, valid ? MessageType.Info : MessageType.Warning);
        }

        private static Color GetCellColor(CellData cell)
        {
            switch (cell.type)
            {
                case CellType.Passenger: return GameColors.ToColor(cell.color);
                case CellType.Obstacle: return new Color(0.35f, 0.35f, 0.35f);
                case CellType.Hole: return new Color(0.1f, 0.1f, 0.1f);
                default: return new Color(0.9f, 0.9f, 0.9f);
            }
        }
    }
}
