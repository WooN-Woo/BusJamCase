using UnityEngine;

namespace BusJam
{
    // object in the LevelEditor scene. the real editor is its inspector / LevelEditorInspector
    public class LevelEditor : MonoBehaviour
    {
        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private int selectedIndex;

        public LevelDatabase LevelDatabase => levelDatabase;

        public int SelectedIndex
        {
            get => selectedIndex;
            set => selectedIndex = value;
        }

        public LevelData SelectedLevel
        {
            get
            {
                if (levelDatabase == null || levelDatabase.Count == 0)
                    return null;

                selectedIndex = Mathf.Clamp(selectedIndex, 0, levelDatabase.Count - 1);
                return levelDatabase.Levels[selectedIndex];
            }
        }
    }
}
