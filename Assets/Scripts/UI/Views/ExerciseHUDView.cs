using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI.Views
{
    [RequireComponent(typeof(UIDocument))]
    public class ExerciseHUDView : MonoBehaviour
    {
        public event Action OnConfirmRequested;
        public event Action OnCalibrateRequested;
        public event Action OnBackRequested;

        private Label _lblRepetitions, _lblFeedback, _lblTimer;
        private VisualElement _btnConfirm, _btnCalibrate, _btnBack, _visualFeedback;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _visualFeedback = root.Q<VisualElement>("visual_feedback");
            _lblRepetitions = root.Q<Label>("label_reps");
            _lblFeedback = root.Q<Label>("label_feedback");
            _lblTimer = root.Q<Label>("label_timer");
            
            _btnConfirm = root.Q<VisualElement>("btn_confirm");
            _btnCalibrate = root.Q<VisualElement>("btn_calibrate");
            _btnBack = root.Q<VisualElement>("btn_back");

            _btnConfirm?.RegisterCallback<ClickEvent>(HandleConfirmClick);
            _btnCalibrate?.RegisterCallback<ClickEvent>(HandleCalibrateClick);
            _btnBack?.RegisterCallback<ClickEvent>(HandleBackClick);
        }

        private void OnDisable()
        {
            _btnConfirm?.UnregisterCallback<ClickEvent>(HandleConfirmClick);
            _btnCalibrate?.UnregisterCallback<ClickEvent>(HandleCalibrateClick);
            _btnBack?.UnregisterCallback<ClickEvent>(HandleBackClick);
        }

        private void HandleConfirmClick(ClickEvent evt) => OnConfirmRequested?.Invoke();
        private void HandleCalibrateClick(ClickEvent evt) => OnCalibrateRequested?.Invoke();
        private void HandleBackClick(ClickEvent evt) => OnBackRequested?.Invoke();

        public void UpdateRepetitionsText(string text)
        {
            if (_lblRepetitions != null) _lblRepetitions.text = text;
        }

        public void ShowWarning(string message)
        {
            if (_visualFeedback == null) return;
            _lblFeedback.text = message;
            _visualFeedback.style.color = new StyleColor(Color.red);
            _visualFeedback.style.display = DisplayStyle.Flex;
        }

        public void HideWarning()
        {
            if (_visualFeedback != null) _visualFeedback.style.display = DisplayStyle.None;
        }

        public void SetConfirmButtonVisibility(bool isVisible)
        {
            if (_btnConfirm != null) 
                _btnConfirm.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}