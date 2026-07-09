using UnityEngine;
using App.Core;
using App.Data.ScriptableObjects;

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(ExerciseMenuView), typeof(UINavigationManager))]
    public class ExerciseMenuController : MonoBehaviour
    {
        [Header("Exercise Definitions")]
        public NeckRotationDefinition neckRotationData;
        public HandGripDefinition handGripData;
        public ShoulderSlideDefinition shoulderSlideData;
        public ElbowFlexionDefinition elbowFlexionData;

        [Header("Destination Scene")]
        public string arSceneName = "App_Exercise";

        private ExerciseMenuView _view;
        private UINavigationManager _navManager;
        private InstructionsController _instructionsController;

        private void Awake()
        {
            _view = GetComponent<ExerciseMenuView>();
            _navManager = GetComponent<UINavigationManager>();

            _view.OnNeckRotationSelected += () => LaunchExercise(neckRotationData);
            _view.OnHandGripSelected += () => LaunchExercise(handGripData);
            _view.OnShoulderSlideSelected += () => LaunchExercise(shoulderSlideData);
            _view.OnElbowFlexionSelected += () => LaunchExercise(elbowFlexionData);
        }

        private void Start()
        {
            _instructionsController = FindFirstObjectByType<InstructionsController>();
            if (_instructionsController == null)
                Debug.LogError("[ExerciseMenuController] InstructionsController não encontrado na cena.");
        }

        private void LaunchExercise(ExerciseDefinition def)
        {
            SessionContext.CurrentExercise = def;
            SessionContext.TargetARScene = arSceneName;
            _instructionsController?.PopulateFromSession();
            _navManager.NavigateTo(AppScreen.Instructions);
        }
    }
}