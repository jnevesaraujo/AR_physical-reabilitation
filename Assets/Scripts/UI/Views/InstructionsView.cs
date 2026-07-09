using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI.Views
{
    [RequireComponent(typeof(UIDocument))]
    public class InstructionsView : MonoBehaviour
    {
        private Label _lblName, _lblDescription;
        private VisualElement _imgTutorial;
        private Button _btnStart;

        public event System.Action OnStartRequested;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _btnStart = root.Q<Button>("btn_start");
            _lblName = root.Q<Label>("lbl_exerciseName");
            _lblDescription = root.Q<Label>("lbl_description");
            _imgTutorial = root.Q<VisualElement>("img_tutorial");

            if (_btnStart != null)
            {
                _btnStart.clicked -= HandleStartClicked;
                _btnStart.clicked += HandleStartClicked;
            }
        }

        private void HandleStartClicked() => OnStartRequested?.Invoke();

        public void SetExerciseName(string name)
        {
            if (_lblName != null) _lblName.text = name;
        }

        public void SetDescription(string description)
        {
            if (_lblDescription == null) return;
            _lblDescription.text = description;
            _lblDescription.style.whiteSpace = WhiteSpace.Normal;
        }
        
        public void SetTutorialImage(Sprite sprite)
        {
            if (_imgTutorial != null && sprite != null)
                _imgTutorial.style.backgroundImage = new StyleBackground(sprite);
        }
    }
}