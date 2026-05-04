using UnityEngine;

namespace App.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "HandGripDefinition", menuName = "RehabApp/Exercises/Hand Grip")]
    public class HandGripDefinition : ExerciseDefinition
    {
        [Header("Hand Grip Biometrics")]
        [Tooltip("Distância euclidiana máxima entre a ponta do polegar (4) e o dedo alvo (8) para registar o fecho da pinça.")]
        public float targetGripDistance = 0.02f;

        [Tooltip("Distância euclidiana mínima para considerar que a mão regressou à posição neutra/aberta.")]
        public float releaseDistance = 0.08f;

        [Header("Pacing Dynamics")]
        [Tooltip("Tempo em segundos que a paciente deve manter a contração isométrica (pinça fechada).")]
        public float isometricHoldTime = 2.0f;
    }
}