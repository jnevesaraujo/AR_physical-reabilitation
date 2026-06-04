using UnityEngine;
using App.Data.ScriptableObjects;

namespace App.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ElbowFlexionDef",
                     menuName = "RehabApp/Exercises/Elbow Flexion")]
    public class ElbowFlexionDefinition : ExerciseDefinition
    {
        [Header("Elbow Flexion Biometrics")]
        [Tooltip("True = right arm, false = left arm.")]
        public bool isRightArm = true;

        [Tooltip("Minimum joint angle (degrees) to count as a full curl (peak).")]
        public float peakAngleThreshold = 60f;

        [Tooltip("Maximum joint angle (degrees) to count as arm fully lowered (rest).")]
        public float restAngleThreshold = 150f;

        [Tooltip("Max allowed deviation of wrist X from shoulder X.")]
        public float horizontalTolerance = 50f;
        
        [Tooltip("Expected arc sweep in degrees from rest to peak. 90-120 is typical.")]
        public float expectedRomDegrees = 100f;
    }
}