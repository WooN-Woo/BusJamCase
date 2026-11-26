using UnityEngine;

namespace BusJam
{
    public enum GameColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange
    }

    public static class GameColors
    {
        public static Color ToColor(GameColor color)
        {
            switch (color)
            {
                case GameColor.Red: return new Color(0.93f, 0.26f, 0.26f);
                case GameColor.Blue: return new Color(0.24f, 0.52f, 0.96f);
                case GameColor.Green: return new Color(0.30f, 0.82f, 0.36f);
                case GameColor.Yellow: return new Color(0.99f, 0.84f, 0.18f);
                case GameColor.Purple: return new Color(0.66f, 0.36f, 0.92f);
                case GameColor.Orange: return new Color(1.00f, 0.56f, 0.16f);
                default: return Color.white;
            }
        }
    }
}
