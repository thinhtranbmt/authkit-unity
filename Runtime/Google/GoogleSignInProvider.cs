// =============================================================================
// Google provider for AuthKit. Compiled ONLY when AUTHKIT_GOOGLE is defined AND the
// Google Sign-In Unity plugin (asmdef "GoogleSignin", package com.google.signin) is
// present. Add AUTHKIT_GOOGLE to your Scripting Define Symbols to enable it.
// =============================================================================
#if AUTHKIT_GOOGLE
using System;
using Cysharp.Threading.Tasks;
using Google;
using UnityEngine;

namespace AuthKit
{
    /// <summary>Project configuration for Google Sign-In (your OAuth web client id, etc.).</summary>
    public sealed class GoogleSignInOptions
    {
        public string WebClientId;
        public string ClientSecret;           // only used in Editor / Standalone
        public bool RequestEmail   = true;
        public bool RequestProfile = true;
        public bool RequestIdToken = true;
        public bool RequestAuthCode = false;
    }

    /// <summary>
    /// Sign in with Google. PLAIN class, no singleton. Pass your WebClientId via
    /// <see cref="GoogleSignInOptions"/> — nothing is hard-coded.
    /// </summary>
    public sealed class GoogleSignInProvider : ISignInProvider
    {
        public AuthProvider Provider => AuthProvider.Google;
        public bool IsSupported => true;

        public GoogleSignInProvider(GoogleSignInOptions options)
        {
            if (options == null || string.IsNullOrEmpty(options.WebClientId))
                Debug.LogWarning("[AuthKit] GoogleSignInOptions.WebClientId is empty — sign-in will likely fail.");

            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestEmail   = options?.RequestEmail   ?? true,
                RequestProfile = options?.RequestProfile ?? true,
                RequestIdToken = options?.RequestIdToken ?? true,
                RequestAuthCode = options?.RequestAuthCode ?? false,
                WebClientId    = options?.WebClientId,
#if UNITY_EDITOR || UNITY_STANDALONE
                ClientSecret   = options?.ClientSecret
#endif
            };
        }

        public async UniTask<AuthResult> SignInAsync()
            => await RunAsync(GoogleSignIn.DefaultInstance.SignIn());

        /// <summary>Attempt to restore a previous session without UI.</summary>
        public async UniTask<AuthResult> SignInSilentlyAsync()
            => await RunAsync(GoogleSignIn.DefaultInstance.SignInSilently());

        private static async UniTask<AuthResult> RunAsync(System.Threading.Tasks.Task<GoogleSignInUser> task)
        {
            try
            {
                GoogleSignInUser user = await task.AsUniTask();
                return AuthResult.Success(Map(user));
            }
            catch (GoogleSignIn.SignInException e) when (e.Status == GoogleSignInStatusCode.CANCELED)
            {
                return AuthResult.Cancelled();
            }
            catch (GoogleSignIn.SignInException e)
            {
                return AuthResult.Failed($"Google Sign-In error: {e.Status} - {e.Message}");
            }
            catch (OperationCanceledException)
            {
                return AuthResult.Cancelled();
            }
            catch (Exception e)
            {
                return AuthResult.Failed("Google Sign-In error: " + e.Message);
            }
        }

        public void SignOut() => GoogleSignIn.DefaultInstance.SignOut();

        private static AuthCredential Map(GoogleSignInUser u) => new AuthCredential
        {
            Provider    = AuthProvider.Google,
            UserId      = u.UserId,
            Email       = u.Email,
            DisplayName = u.DisplayName,
            IdToken     = u.IdToken,
            AuthorizationCode = u.AuthCode ?? string.Empty,
            ImageUrl    = u.ImageUrl != null ? u.ImageUrl.ToString() : string.Empty
        };
    }
}
#endif
