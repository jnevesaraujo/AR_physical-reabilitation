using System;
using UnityEngine;

namespace App.Controllers
{
    public class ExerciseTimerController : MonoBehaviour
    {
        private float _elapsedTime;
        private bool _isRunning;

        public int ElapsedSeconds => Mathf.FloorToInt(_elapsedTime);
        public event Action<int> OnTimeUpdated;

        private void Update()
        {
            if (!_isRunning) return;

            int previousSeconds = ElapsedSeconds;
            _elapsedTime += Time.deltaTime;

            if (ElapsedSeconds > previousSeconds)
            {
                OnTimeUpdated?.Invoke(ElapsedSeconds);
            }
        }

        public void StartTimer() => _isRunning = true;
        
        public void PauseTimer() => _isRunning = false;
        
        public void ResetTimer()
        {
            _elapsedTime = 0f;
            _isRunning = false;
            OnTimeUpdated?.Invoke(0);
        }
    }
}