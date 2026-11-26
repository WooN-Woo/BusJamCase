using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusJam
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float exitOffset = 0.9f;
        [SerializeField] private float topSpaceForUI = 2f;

        [Header("Prefabs")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private Passenger passengerPrefab;

        [Header("References")]
        [SerializeField] private WaitingArea waitingArea;
        [SerializeField] private BusStation busStation;

        private readonly List<GameObject> spawnedObjects = new List<GameObject>();
        private readonly List<Passenger> passengers = new List<Passenger>();
        private Passenger[,] grid;
        private bool[,] obstacles;
        private int width;
        private int height;
        private int movingPassengers;
        private bool isFinished;

        public event Action Completed;
        public event Action<string> Failed;

        private void OnEnable()
        {
            busStation.BusArrived += OnBusArrived;
            busStation.AllBusesLeft += OnAllBusesLeft;
        }

        private void OnDisable()
        {
            busStation.BusArrived -= OnBusArrived;
            busStation.AllBusesLeft -= OnAllBusesLeft;
        }

        public void Build(LevelData level)
        {
            Clear();

            width = level.Width;
            height = level.Height;
            grid = new Passenger[width, height];
            obstacles = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 position = GetCellPosition(x, y);
                    CellData cell = level.GetCell(x, y);
                    if (cell.type == CellType.Hole)
                    {
                        obstacles[x, y] = true;
                        continue;
                    }

                    spawnedObjects.Add(Instantiate(tilePrefab, position, Quaternion.identity, transform));

                    if (cell.type == CellType.Obstacle)
                    {
                        obstacles[x, y] = true;
                        spawnedObjects.Add(Instantiate(obstaclePrefab, position, Quaternion.identity, transform));
                    }
                    else if (cell.type == CellType.Passenger)
                    {
                        Passenger passenger = Instantiate(passengerPrefab, position, Quaternion.Euler(0f, 180f, 0f), transform);
                        passenger.Setup(cell.color, new Vector2Int(x, y));
                        grid[x, y] = passenger;
                        passengers.Add(passenger);
                    }
                }
            }

            waitingArea.Build();
            busStation.Build(level.BusColors);
            RefreshReachablePassengers();
        }

        public void Clear()
        {
            foreach (GameObject spawned in spawnedObjects)
                Destroy(spawned);
            spawnedObjects.Clear();

            foreach (Passenger passenger in passengers)
            {
                if (passenger != null)
                    Destroy(passenger.gameObject);
            }
            passengers.Clear();

            waitingArea.Clear();
            busStation.Clear();
            movingPassengers = 0;
            isFinished = false;
        }

        // called from outside when time is up
        public void Stop()
        {
            isFinished = true;
        }

        public void HandleTap(Passenger passenger)
        {
            if (isFinished || !passenger.IsOnGrid)
                return;

            List<Vector2Int> path = GridPathfinder.FindPathToFrontRow(GetBlockedCells(), passenger.GridPosition);
            if (path == null)
            {
                passenger.PlayBlockedShake();
                return;
            }

            Bus bus = busStation.CurrentBus;
            bool goesToBus = bus != null && bus.BusColor == passenger.PassengerColor && bus.HasFreeSeat;
            int slotIndex = -1;
            if (!goesToBus)
            {
                slotIndex = waitingArea.FindFreeSlot();
                if (slotIndex < 0)
                    return;
            }

            grid[passenger.GridPosition.x, passenger.GridPosition.y] = null;
            passenger.LeaveGrid();
            RefreshReachablePassengers();

            List<Vector3> route = BuildRoute(path);
            movingPassengers++;

            if (goesToBus)
            {
                bus.ReserveSeat();
                route.Add(bus.DoorPosition);
                passenger.MoveAlong(route, () => OnPassengerReachedBus(passenger, bus));
            }
            else
            {
                waitingArea.Reserve(slotIndex, passenger);
                route.Add(waitingArea.GetSlotPosition(slotIndex));
                passenger.MoveAlong(route, OnPassengerReachedSlot);
            }
        }

        // area the camera needs to show
        public Bounds GetPlayAreaBounds()
        {
            Bounds bounds = new Bounds(GetCellPosition(0, 0), Vector3.zero);
            bounds.Encapsulate(GetCellPosition(width - 1, height - 1));
            bounds.Encapsulate(waitingArea.transform.position);
            bounds.Encapsulate(busStation.transform.position + Vector3.forward * topSpaceForUI);
            bounds.Expand(new Vector3(cellSize, 1f, cellSize));
            return bounds;
        }

        private void OnPassengerReachedBus(Passenger passenger, Bus bus)
        {
            movingPassengers--;
            if (isFinished)
                return;

            bus.SeatPassenger();
            passengers.Remove(passenger);
            Destroy(passenger.gameObject);

            if (bus.IsFull)
                busStation.DepartCurrentBus();

            CheckFail();
        }

        private void OnPassengerReachedSlot()
        {
            movingPassengers--;
            if (isFinished)
                return;

            BoardWaitingPassengers();
            CheckFail();
        }

        private void OnBusArrived()
        {
            if (isFinished)
                return;

            BoardWaitingPassengers();
            CheckFail();
        }

        private void OnAllBusesLeft()
        {
            if (isFinished)
                return;

            isFinished = true;
            Completed?.Invoke();
        }

        private void BoardWaitingPassengers()
        {
            Bus bus = busStation.CurrentBus;
            if (bus == null)
                return;

            while (bus.HasFreeSeat)
            {
                Passenger passenger = waitingArea.FindWaitingPassenger(bus.BusColor);
                if (passenger == null)
                    return;

                waitingArea.Remove(passenger);
                bus.ReserveSeat();
                movingPassengers++;
                passenger.MoveAlong(new List<Vector3> { bus.DoorPosition }, () => OnPassengerReachedBus(passenger, bus));
            }
        }

        // waiting slots full = fail
        // but let everyone sit down first and the bus arrive, so matching ones can still get on
        private void CheckFail()
        {
            if (isFinished || movingPassengers > 0 || !waitingArea.IsFull)
                return;

            if (busStation.CurrentBus == null)
                return;

            isFinished = true;
            Failed?.Invoke("No space left!");
        }

        private void RefreshReachablePassengers()
        {
            bool[,] blocked = GetBlockedCells();
            foreach (Passenger passenger in passengers)
            {
                if (passenger.IsOnGrid)
                    passenger.SetReachable(GridPathfinder.FindPathToFrontRow(blocked, passenger.GridPosition) != null);
            }
        }

        private bool[,] GetBlockedCells()
        {
            bool[,] blocked = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    blocked[x, y] = obstacles[x, y] || grid[x, y] != null;
            }
            return blocked;
        }

        // cells to world positions + one step out of the grid at the end
        private List<Vector3> BuildRoute(List<Vector2Int> path)
        {
            List<Vector3> route = new List<Vector3>();
            foreach (Vector2Int cell in path)
                route.Add(GetCellPosition(cell.x, cell.y));

            Vector3 frontCell = route[route.Count - 1];
            route.Add(frontCell + Vector3.forward * exitOffset);
            return route;
        }

        private Vector3 GetCellPosition(int x, int y)
        {
            float offsetX = (width - 1) * 0.5f;
            return transform.position + new Vector3((x - offsetX) * cellSize, 0f, -y * cellSize);
        }
    }
}
