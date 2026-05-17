using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class ExerciseMenuPresenter : MonoBehaviour
    {
        [SerializeField] private ExerciseMenuManager _exerciseMenuManager;
        private VisualElement _root;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            if (_exerciseMenuManager == null)
            {
                Debug.LogError("[UI] ExerciseMenuManager não está atribuído no Inspector.");
                return;
            }

            BindExerciseButtons();
        }

        private void BindExerciseButtons()
        {
            var btnNeckRotation = _root.Q<VisualElement>("btn-neckRotation");
            btnNeckRotation.RegisterCallback<ClickEvent>(_ => _exerciseMenuManager.OnClick_LaunchNeckRotation());

            var btnHandGrip = _root.Q<VisualElement>("btn-handGrip");
            btnHandGrip.RegisterCallback<ClickEvent>(_ => _exerciseMenuManager.OnClick_LaunchHandGrip());

            var btnShoulderSlide = _root.Q<VisualElement>("btn-shoulderSlide");
            btnShoulderSlide.RegisterCallback<ClickEvent>(_ => _exerciseMenuManager.OnClick_LaunchShoulderSlide());
        }
    }
}