using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusJam
{
    // x = column, y = row. y = 0 is the front row (bus side)
    [CreateAssetMenu(fileName = "Level_", menuName = "BusJam/Level Data")]
    public class LevelData : ScriptableObject
    {
        public const int MinSize = 3;
        public const int MaxSize = 12;
        public const int BusCapacity = 3;

        [SerializeField] private int width = 5;
        [SerializeField] private int height = 5;
        [SerializeField] private float timeLimit = 60f;
        [SerializeField] private List<GameColor> busColors = new List<GameColor>();
        [SerializeField] private CellData[] cells = new CellData[25];

        public int Width => width;
        public int Height => height;
        public float TimeLimit => timeLimit;
        public List<GameColor> BusColors => busColors;

        public CellData GetCell(int x, int y)
        {
            return cells[y * width + x];
        }

        public void SetCell(int x, int y, CellData cell)
        {
            cells[y * width + x] = cell;
        }

        public void SetTimeLimit(float seconds)
        {
            timeLimit = Mathf.Max(5f, seconds);
        }

        // on resize, keep the cells that still fit
        public void Resize(int newWidth, int newHeight)
        {
            newWidth = Mathf.Clamp(newWidth, MinSize, MaxSize);
            newHeight = Mathf.Clamp(newHeight, MinSize, MaxSize);

            CellData[] newCells = new CellData[newWidth * newHeight];
            for (int y = 0; y < Mathf.Min(height, newHeight); y++)
            {
                for (int x = 0; x < Mathf.Min(width, newWidth); x++)
                {
                    newCells[y * newWidth + x] = cells[y * width + x];
                }
            }

            width = newWidth;
            height = newHeight;
            cells = newCells;
        }

        public int CountPassengers(GameColor color)
        {
            int count = 0;
            foreach (CellData cell in cells)
            {
                if (cell.type == CellType.Passenger && cell.color == color)
                    count++;
            }
            return count;
        }

        // 1 bus for every 3 passengers of a color, colors mixed in turn
        public void AutoFillBuses()
        {
            busColors.Clear();

            GameColor[] allColors = (GameColor[])Enum.GetValues(typeof(GameColor));
            int[] remaining = new int[allColors.Length];
            for (int i = 0; i < allColors.Length; i++)
                remaining[i] = CountPassengers(allColors[i]) / BusCapacity;

            bool added = true;
            while (added)
            {
                added = false;
                for (int i = 0; i < allColors.Length; i++)
                {
                    if (remaining[i] == 0)
                        continue;

                    busColors.Add(allColors[i]);
                    remaining[i]--;
                    added = true;
                }
            }
        }

        // each color needs exactly as many seats as passengers
        public bool IsValid(out string message)
        {
            if (cells == null || cells.Length != width * height)
            {
                message = "Cell count does not match the grid size.";
                return false;
            }

            if (busColors.Count == 0)
            {
                message = "Add at least one bus.";
                return false;
            }

            foreach (GameColor color in Enum.GetValues(typeof(GameColor)))
            {
                int passengers = CountPassengers(color);
                int seats = 0;
                foreach (GameColor busColor in busColors)
                {
                    if (busColor == color)
                        seats += BusCapacity;
                }

                if (passengers != seats)
                {
                    message = $"{color}: {passengers} passengers but {seats} seats.";
                    return false;
                }
            }

            message = "Level is valid.";
            return true;
        }
    }
}
