using UnityEngine;

namespace App.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ElbowFlexionDef", menuName = "RehabApp/Exercises/Elbow Flexion")]
    public class ElbowFlexionDefinition : ExerciseDefinition
    {
        [Header("Biomechanics")]
        [Tooltip("Maximum angle (extended arm) tolerated to initiate repetition.")]
        public float extensionThresholdAngle = 160f; 
        
        [Tooltip("Minimum angle (flexed arm) required to validate repetition.")]
        public float flexionTargetAngle = 40f;

        [Tooltip("Time in seconds that the user must hold the flexion to count as a valid repetition.")]
        public float holdTimeSeconds = 2f;
    }
}