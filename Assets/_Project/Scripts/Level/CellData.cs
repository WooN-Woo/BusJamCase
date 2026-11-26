using System;

namespace BusJam
{
    public enum CellType
    {
        Empty,
        Passenger,
        Obstacle,
        Hole
    }

    [Serializable]
    public struct CellData
    {
        public CellType type;
        public GameColor color;
    }
}
