using UnityEngine;

namespace App.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "HandGripDefinition", menuName = "RehabApp/Exercises/Hand Grip")]
    public class HandGripDefinition : ExerciseDefinition
    {
        [Header("Hand Grip Biometrics")]
        [Tooltip("Maximum Euclidean distance between the thumb tip (4) and the target finger (8) to register the pinch closure.")]
        public float targetGripDistance = 0.02f;

        [Tooltip("Minimum Euclidean distance for considering that the hand has returned to the neutral/open position.")]
        public float releaseDistance = 0.08f;

        [Header("Pacing Dynamics")]
        [Tooltip("Time in seconds that the user must hold the grip to count as a valid repetition.")]
        public float isometricHoldTime = 2.0f;
    }
}