using System;

namespace App.Data.Models
{
    [Serializable]
    public class UserProfile
    {
        public string userId;
        public string fullName;
        public string email;
        public DateTime registrationDate;
        public int totalSessionsCompleted;
        public string affectedSide;
    }
}