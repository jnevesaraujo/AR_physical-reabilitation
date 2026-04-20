using UnityEngine;
using App.Data.ScriptableObjects;

namespace App.Vision
{
    public class ARExerciseVisualizer : MonoBehaviour
    {
        [Header("Visual Settings")]
        public float visualScaleMultiplier = 100f;
        public float zOffset = -2f;
        private GameObject _activeGuide;
        private ARGuideController _guideController;
        private Vector3 _anchorOffset;
        
        public void InitializeGuide(ExerciseDefinition def, Vector3 startPosition)
        {
            if (def is NeckRotationDefinition neckDef)
            {
                CleanUp();

                float visualRadius = neckDef.minimumRotationAmplitude * visualScaleMultiplier;

                if (neckDef.visualGuidePrefab != null)
                {
                    _activeGuide = Instantiate(neckDef.visualGuidePrefab);
                    _anchorOffset = new Vector3(0, 0, zOffset);

                    _activeGuide.transform.position = new Vector3(
                        startPosition.x,
                        startPosition.y,
                        startPosition.z + zOffset
                    );

                    _guideController = _activeGuide.GetComponent<ARGuideController>();
                    if (_guideController != null)
                    {
                        _guideController.InitializeGuide(visualRadius, neckDef.targetSecondsPerRep, startPosition);
                    }
                }
            }

        }

        public void UpdatePacerFeedback(Vector3 currentNosePosition)
        {
            if (_guideController != null)
                _guideController.EvaluateSynchronization(currentNosePosition);
        }

        public void SetFeedbackMode(bool isCorrectPosture)
        {
            if (_guideController != null)
            {
                _guideController.SetColor(isCorrectPosture ? Color.green : Color.red);
            }
        }

        private void CleanUp()
        {
            if (_activeGuide != null) Destroy(_activeGuide);
            _guideController = null;
        }
    }
}