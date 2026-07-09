using UnityEngine;
using UnityEngine.SceneManagement;
using App.Core;
using App.UI.Views;

namespace App.UI.Toolkit
{
    [RequireComponent(typeof(InstructionsView))]
    public class InstructionsController : MonoBehaviour
    {
        private InstructionsView _view;

        private void Awake()
        {
            _view = GetComponent<InstructionsView>();
            _view.OnStartRequested += HandleStartRequested;
        }

        private void OnDestroy() { if (_view != null) _view.OnStartRequested -= HandleStartRequested; }

        public void PopulateFromSession()
        {
            var exercise = SessionContext.CurrentExercise;
            if (exercise == null) return;
            _view.SetExerciseName(exercise.exerciseName);
            _view.SetDescription(exercise.description);
            _view.SetTutorialImage(exercise.tutorialIcon);
        }

        private void HandleStartRequested()
        {
            string targetScene = SessionContext.TargetARScene;
            if (!string.IsNullOrEmpty(targetScene))
                SceneManager.LoadScene(targetScene);
            else
                Debug.LogError("[InstructionsController] Cena de destino não encontrada.");
        }
    }
}