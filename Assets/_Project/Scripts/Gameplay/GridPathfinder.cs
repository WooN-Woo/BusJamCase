using System.Collections.Generic;
using UnityEngine;

namespace BusJam
{
    // BFS: check if a passenger can get out to the front row (y = 0)
    public static class GridPathfinder
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up
        };

        // blocked = walls, holes and cells with a passenger
        // returns the cells to walk through, or null if there is no way
        public static List<Vector2Int> FindPathToFrontRow(bool[,] blocked, Vector2Int start)
        {
            int width = blocked.GetLength(0);
            int height = blocked.GetLength(1);

            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            cameFrom[start] = start;
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (current.y == 0)
                    return BuildPath(cameFrom, start, current);

                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int next = current + direction;
                    bool inside = next.x >= 0 && next.x < width && next.y >= 0 && next.y < height;
                    if (!inside || blocked[next.x, next.y] || cameFrom.ContainsKey(next))
                        continue;

                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }

            return null;
        }

        private static List<Vector2Int> BuildPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int end)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int current = end;
            while (current != start)
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Add(start);
            path.Reverse();
            return path;
        }
    }
}
