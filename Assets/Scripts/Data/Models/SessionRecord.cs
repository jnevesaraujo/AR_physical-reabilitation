using System;
using System.Collections.Generic;

namespace App.Data.Models
{
    [Serializable]
    public class SessionRecord
    {
        public string sessionId;
        public string exerciseId;
        public DateTime sessionTimestamp;   
        public int completedReps;
        public int targetReps;
        public float accuracyScore; 
        public float durationSeconds;
        public bool isCompleted;
    }
}