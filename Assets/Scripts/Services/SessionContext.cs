using App.Data.ScriptableObjects;
using UnityEngine;

namespace Services
{
    public static class SessionContext
    {
        // Armazena a definição do exercício selecionado no menu
        public static ExerciseDefinition CurrentExercise { get; set; }
        
        public static void Clear()
        {
            CurrentExercise = null;
        }
    }
}