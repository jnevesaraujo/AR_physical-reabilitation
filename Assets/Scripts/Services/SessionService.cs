using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using App.Data.Models;
using System;
using App.Core;

namespace App.Services
{
    public class SessionService
    {
        private FirebaseFirestore _db;
        private string _userId;

        public void Initialize(string userId)
        {
            _db = FirebaseFirestore.DefaultInstance;
            _userId = userId;
        }

        public async Task SaveSessionAsync(SessionRecord session)
        {
            var docRef = _db
                .Collection("users")
                .Document(_userId)
                .Collection("sessions")
                .Document();

            session.sessionId = docRef.Id;

            var subjectId = SessionContext.CurrentUser?.subjectId ?? "unknown";

            var data = new Dictionary<string, object>
            {
                { "subjectId",       subjectId},
                { "sessionId",       session.sessionId },
                { "exerciseId",      session.exerciseId },
                { "sessionTimestamp", session.sessionTimestamp.ToString("o") },
                { "completedReps",   session.completedReps },
                { "targetReps",      session.targetReps },
                { "accuracyScore",   session.accuracyScore },
                { "durationSeconds", session.durationSeconds },
                { "isCompleted",     session.isCompleted }
            };

            await docRef.SetAsync(data);
        }

        public async Task<List<SessionRecord>> GetSessionHistoryAsync()
        {
            var snapshot = await _db
                .Collection("users")
                .Document(_userId)
                .Collection("sessions")
                .OrderByDescending("date")
                .Limit(20)
                .GetSnapshotAsync();

            var sessions = new List<SessionRecord>();
            foreach (var doc in snapshot.Documents)
            {
                sessions.Add(new SessionRecord
                {
                    sessionId = doc.Id,
                    exerciseId = doc.GetValue<string>("exerciseId"),
                    sessionTimestamp = DateTime.Parse(doc.GetValue<string>("sessionTimestamp")),
                    completedReps = doc.GetValue<int>("completedReps"),
                    targetReps = doc.ContainsField("targetReps") ? doc.GetValue<int>("targetReps") : 0,
                    // Firestore frequently stores floats as doubles, explicit conversion prevents it
                    accuracyScore = Convert.ToSingle(doc.GetValue<double>("accuracyScore")),
                    durationSeconds = Convert.ToSingle(doc.GetValue<double>("durationSeconds")),
                    isCompleted = doc.ContainsField("isCompleted") && doc.GetValue<bool>("isCompleted")
                });
            }
            return sessions;
        }
    }
}