using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class ExerciseTimerController : MonoBehaviour
    {
        private Label _lblTimer;
        private IVisualElementScheduledItem _timerTask;
        private int _elapsedSeconds = 0;
        public int ElapsedSeconds => _elapsedSeconds;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            

            _lblTimer = root.Q<Label>("lbl_timer");

            if (_lblTimer != null)
            {
                // Initial timer text setup
                UpdateTimerText();

                // Registers a scheduled task to increment the timer every second
                _timerTask = _lblTimer.schedule.Execute(IncrementTime).Every(1000);
                
                // Pause the timer initially until StartTimer is called
                _timerTask.Pause(); 
            }
        }

        private void IncrementTime()
        {
            _elapsedSeconds++;
            UpdateTimerText();
        }

        private void UpdateTimerText()
        {
            TimeSpan time = TimeSpan.FromSeconds(_elapsedSeconds);
            _lblTimer.text = time.ToString(@"mm\:ss");
        }

        // Public methods to control the timer externally
        public void StartTimer()
        {
            _timerTask?.Resume();
        }

        public void PauseTimer()
        {
            _timerTask?.Pause();
        }

        public void ResetTimer()
        {
            _elapsedSeconds = 0;
            UpdateTimerText();
        }

        private void OnDisable()
        {
            // Memory cleanup: Stop the timer when the component is disabled
            _timerTask?.Pause();
        }
    }
}