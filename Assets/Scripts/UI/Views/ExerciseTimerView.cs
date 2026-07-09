using System;
using UnityEngine;
using UnityEngine.UIElements;
using App.Controllers; // Ajuste consoante o namespace adotado

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument), typeof(ExerciseTimerController))]
    public class ExerciseTimerView : MonoBehaviour
    {
        private Label _lblTimer;
        private ExerciseTimerController _controller;

        private void Awake()
        {
            _controller = GetComponent<ExerciseTimerController>();
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _lblTimer = root.Q<Label>("lbl_timer");

            if (_controller != null)
            {
                _controller.OnTimeUpdated += UpdateTimerText;
                UpdateTimerText(_controller.ElapsedSeconds);
            }
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.OnTimeUpdated -= UpdateTimerText;
            }
        }

        private void UpdateTimerText(int seconds)
        {
            if (_lblTimer == null) return;
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            _lblTimer.text = time.ToString(@"mm\:ss");
        }
    }
}