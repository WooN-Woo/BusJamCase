using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusJam
{
    // lines up the buses, when one is full it leaves and the next one comes
    public class BusStation : MonoBehaviour
    {
        [SerializeField] private Bus busPrefab;
        [SerializeField] private float queueSpacing = 3f;
        [SerializeField] private float exitDistance = 14f;

        private readonly List<Bus> buses = new List<Bus>();

        public event Action BusArrived;
        public event Action AllBusesLeft;

        public Bus CurrentBus
        {
            get
            {
                if (buses.Count > 0 && buses[0].IsAtStop)
                    return buses[0];
                return null;
            }
        }

        public void Build(List<GameColor> busColors)
        {
            Clear();

            for (int i = 0; i < busColors.Count; i++)
            {
                Bus bus = Instantiate(busPrefab, GetQueuePosition(i), Quaternion.identity, transform);
                bus.Setup(busColors[i]);
                buses.Add(bus);
            }

            if (buses.Count > 0)
                buses[0].IsAtStop = true;
        }

        public void Clear()
        {
            foreach (Bus bus in buses)
            {
                if (bus != null)
                    Destroy(bus.gameObject);
            }
            buses.Clear();
        }

        public void DepartCurrentBus()
        {
            Bus leavingBus = buses[0];
            buses.RemoveAt(0);
            leavingBus.IsAtStop = false;
            leavingBus.MoveTo(transform.position + Vector3.right * exitDistance, () => Destroy(leavingBus.gameObject));

            if (buses.Count == 0)
            {
                AllBusesLeft?.Invoke();
                return;
            }

            for (int i = 0; i < buses.Count; i++)
            {
                Bus bus = buses[i];
                if (i == 0)
                    bus.MoveTo(GetQueuePosition(0), () => OnBusReachedStop(bus));
                else
                    bus.MoveTo(GetQueuePosition(i), null);
            }
        }

        private void OnBusReachedStop(Bus bus)
        {
            bus.IsAtStop = true;
            BusArrived?.Invoke();
        }

        // 0 = the stop, the rest wait in line on the left
        private Vector3 GetQueuePosition(int index)
        {
            return transform.position + Vector3.left * (index * queueSpacing);
        }
    }
}
