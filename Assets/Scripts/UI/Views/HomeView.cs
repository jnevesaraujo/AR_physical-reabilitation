// HomeView.cs
using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI.Views
{
    [RequireComponent(typeof(UIDocument))]
    public class HomeView : MonoBehaviour
    {
        private VisualElement _root;
        private Label _lblUsername, _lblSubjectId, _lblSessionsCompleted, _lblAffectedSide;
        private ProgressBar _progressBar;
        private VisualElement _btnGoExercises;

        public event System.Action OnGoExercisesRequested;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _lblUsername = _root.Q<Label>("home_lbl_username");
            _lblSubjectId = _root.Q<Label>("subject_id_home_label");
            _lblSessionsCompleted = _root.Q<Label>("session_completed");
            _lblAffectedSide = _root.Q<Label>("lbl_affected-side");
            _progressBar = _root.Q<ProgressBar>("SessionProgressBar");
            _btnGoExercises = _root.Q<VisualElement>("go_exercises_btn");

            _btnGoExercises?.AddTouchFeedback();
            _btnGoExercises?.RegisterCallback<ClickEvent>(OnGoExercisesClick);
        }

        private void OnDisable() => _btnGoExercises?.UnregisterCallback<ClickEvent>(OnGoExercisesClick);

        private void OnGoExercisesClick(ClickEvent evt) => OnGoExercisesRequested?.Invoke();

        public void SetUsername(string greeting)
        {
            if (_lblUsername != null) _lblUsername.text = greeting;
        }

        public void SetSubjectId(string text)
        {
            if (_lblSubjectId != null) _lblSubjectId.text = text;
        }

        public void SetSessionsCompletedText(string text)
        {
            if (_lblSessionsCompleted != null) _lblSessionsCompleted.text = text;
        }

        public void SetProgress(float percent, string title)
        {
            if (_progressBar == null) return;
            _progressBar.value = Mathf.Clamp(percent, 0f, 100f);
            _progressBar.title = title;
        }
        
        public void SetAffectedSideText(string text)
        {
            if (_lblAffectedSide != null) _lblAffectedSide.text = text;
        }
    }
}