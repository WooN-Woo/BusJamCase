using System.Collections.Generic;
using UnityEngine;

namespace BusJam
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "BusJam/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        public List<LevelData> Levels => levels;
        public int Count => levels.Count;

        public bool HasLevel(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex < levels.Count;
        }

        public LevelData GetLevel(int levelIndex)
        {
            return levels[levelIndex];
        }
    }
}
