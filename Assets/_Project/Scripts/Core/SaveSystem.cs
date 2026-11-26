using UnityEngine;

namespace BusJam
{
    public static class SaveSystem
    {
        private const string LevelIndexKey = "LevelIndex";

        public static int LevelIndex
        {
            get => PlayerPrefs.GetInt(LevelIndexKey, 0);
            set
            {
                PlayerPrefs.SetInt(LevelIndexKey, value);
                PlayerPrefs.Save();
            }
        }
    }
}
