using System;
using UnityEngine;

namespace BusJam
{
    public class LevelTimer : MonoBehaviour
    {
        private float remainingTime;
        private bool isRunning;

        public float RemainingTime => remainingTime;

        public event Action Expired;

        public void Begin(float seconds)
        {
            remainingTime = seconds;
            isRunning = true;
        }

        public void Stop()
        {
            isRunning = false;
        }

        private void Update()
        {
            if (!isRunning)
                return;

            remainingTime -= Time.deltaTime;
            if (remainingTime > 0f)
                return;

            remainingTime = 0f;
            isRunning = false;
            Expired?.Invoke();
        }
    }
}
