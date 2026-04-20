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
        public void InitializeGuide(ExerciseDefinition def, Vector3 startPosition)
        {
            if (def is NeckRotationDefinition neckDef)
            {
                CleanUp();

                float visualRadius = neckDef.minimumRotationAmplitude * visualScaleMultiplier;

                if (neckDef.visualGuidePrefab != null)
                {
                    _activeGuide = Instantiate(neckDef.visualGuidePrefab);
                    
                    _activeGuide.transform.position = new Vector3(
                        startPosition.x, 
                        startPosition.y, 
                        startPosition.z + zOffset
                    );
                    
                    _guideController = _activeGuide.GetComponent<ARGuideController>();
                    if (_guideController != null)
                    {
                        _guideController.InitializeGuide(visualRadius, neckDef.targetSecondsPerRep);
                    }
                }
            }
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
        }
    }
}