using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class SummaryView : MonoBehaviour
    {
        private VisualElement _root;
        private Label _lblExerciseName, _lblTimeResult, _lblRepResult, _lblStatusInfo;
        private Button _btnCloseSummary;

        // Controller subscribes to this event to handle the close action
        public event Action OnCloseRequested;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _lblExerciseName = _root.Q<Label>("summary_exercise_name");
            _lblTimeResult = _root.Q<Label>("summary_time_result");
            _lblRepResult = _root.Q<Label>("summary_rep_result");
            _lblStatusInfo = _root.Q<Label>("summary_status_info");
            _btnCloseSummary = _root.Q<Button>("btn_close_summary");

            _btnCloseSummary?.RegisterCallback<ClickEvent>(_ => OnCloseRequested?.Invoke());
        }

        public void SetExerciseName(string name)
        {
            if (_lblExerciseName != null) _lblExerciseName.text = name;
        }

        public void SetResults(string time, string reps)
        {
            if (_lblTimeResult != null) _lblTimeResult.text = time;
            if (_lblRepResult != null) _lblRepResult.text = reps;
        }

        public void SetStatusMessage(string message)
        {
            if (_lblStatusInfo != null) _lblStatusInfo.text = message;
        }
    }
}