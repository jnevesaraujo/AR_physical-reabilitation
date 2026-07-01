using Firebase.Auth;
using System.Threading.Tasks;
using UnityEngine;
using App.Core;
using UserProfile = App.Data.Models.UserProfile;
using System;
using App.Data.Models;

namespace App.Services
{
    public class AuthService
    {
        private FirebaseAuth _auth;
        private ProfileService _profileService;

        public void Initialize()
        {
            _auth = FirebaseAuth.DefaultInstance;
            _profileService = new ProfileService();
            Debug.Log($"[AuthService] Auth initialized: {_auth != null}");
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                AuthResult authResult = await _auth.SignInWithEmailAndPasswordAsync(email, password);
                FirebaseUser firebaseUser = authResult.User;
                UserProfile userProfile = await _profileService.LoadProfileAsync(firebaseUser.UserId);
                SessionContext.CurrentUser = userProfile; // Cache profile for use across scenes
                Debug.Log($"[Auth] Login successful: {email} (UserName: {userProfile.firstName} {userProfile.lastName})");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Auth] Login failed: {e.Message}");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            try
            {
                await _auth.CreateUserWithEmailAndPasswordAsync(email, password);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Auth] Register failed: {e.Message}");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string email, string password,
                                       string firstName, string lastName)
        {
            Debug.Log($"email: {email ?? "NULL"}");
            Debug.Log($"password: {password ?? "NULL"}");
            Debug.Log($"firstName: {firstName ?? "NULL"}");
            Debug.Log($"lastName: {lastName ?? "NULL"}");
            Debug.Log($"_auth is null: {_auth == null}");

            try
            {
                var result = await _auth.CreateUserWithEmailAndPasswordAsync(email, password);

                // Create Firestore profile immediately after auth
                var profile = new UserProfile
                {
                    userId = result.User.UserId,
                    firstName = firstName,
                    lastName = lastName,
                    email = email,
                    registrationDate = DateTime.UtcNow,
                    totalSessionsCompleted = 0,
                    affectedSide = AffectedSide.Unknown // set later in profile setup
                };

                var profileService = new ProfileService();
                await profileService.CreateProfileAsync(profile);

                // Cache in SessionContext for use across scenes
                SessionContext.CurrentUser = profile;

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Auth] Register failed: {e.Message}");
                return false;
            }
        }

        public void Logout() => _auth.SignOut();

        public bool IsLoggedIn => _auth.CurrentUser != null;
        public string UserId => _auth.CurrentUser?.UserId;
    }
}