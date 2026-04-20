using UnityEngine;

namespace App.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NeckRotationDef", menuName = "RehabApp/Exercises/Neck Rotation")]
    public class NeckRotationDefinition : ExerciseDefinition
    {
        [Header("Neck Specific Biometrics")]
        [Tooltip("Maximum allowed shoulder height difference before triggering a warning.")]
        public float shoulderAlignmentTolerance = 0.6f;
        
        [Tooltip("Minimum radius (distance from nose to center) to be considered a valid rotation.")]
        public float minimumRotationAmplitude = 0.08f;

        [Tooltip("Radius around the origin to consider the head is back in the neutral/rest position.")]
        public float neutralZoneRadius = 0.05f;

        [Header("Pacing Dynamics")]
        [Tooltip("Time in seconds the patient should take to complete one full 360 degree rotation.")]
        public float targetSecondsPerRep = 4.0f;
    }
}