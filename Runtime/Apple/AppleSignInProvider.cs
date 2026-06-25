// =============================================================================
// Apple provider for AuthKit. Compiled ONLY when AUTHKIT_APPLE is defined AND the
// Apple Sign-In Unity plugin (asmdef "AppleAuth") is present. Add AUTHKIT_APPLE to
// your Scripting Define Symbols to enable it.
// =============================================================================
#if AUTHKIT_APPLE
using System;
using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AuthKit
{
    /// <summary>
    /// Sign in with Apple. PLAIN class, no singleton, no MonoBehaviour — the Apple SDK
    /// needs <c>AppleAuthManager.Update()</c> pumped each frame, so we pump it via a
    /// UniTask loop only for the duration of the sign-in (no scene object required).
    /// </summary>
    public sealed class AppleSignInProvider : ISignInProvider
    {
        private readonly IAppleAuthManager _manager;

        public AuthProvider Provider => AuthProvider.Apple;
        public bool IsSupported => AppleAuthManager.IsCurrentPlatformSupported;

        public AppleSignInProvider()
        {
            if (AppleAuthManager.IsCurrentPlatformSupported)
                _manager = new AppleAuthManager(new PayloadDeserializer());
            else
                Debug.LogWarning("[AuthKit] Apple Sign-In not supported on this platform.");
        }

        public async UniTask<AuthResult> SignInAsync()
        {
            if (_manager == null)
                return AuthResult.Failed("Apple Sign-In not supported / not initialized.");

            bool done = false;
            AuthResult result = null;

            var args = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

            _manager.LoginWithAppleId(
                args,
                credential =>
                {
                    if (credential is IAppleIDCredential apple)
                        result = AuthResult.Success(Map(apple));
                    else
                        result = AuthResult.Failed("Invalid Apple credential.");
                    done = true;
                },
                error =>
                {
                    var code = error.GetAuthorizationErrorCode();
                    result = code == AuthorizationErrorCode.Canceled
                        ? AuthResult.Cancelled()
                        : AuthResult.Failed("Apple Sign-In error: " + code);
                    done = true;
                });

            // Pump the SDK until the native callback resolves.
            while (!done)
            {
                _manager.Update();
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            return result;
        }

        public void SignOut()
        {
            // Apple has no client-side sign-out; the app clears its own session.
        }

        private static AuthCredential Map(IAppleIDCredential c)
        {
            string idToken = c.IdentityToken != null ? Encoding.UTF8.GetString(c.IdentityToken) : string.Empty;
            string authCode = c.AuthorizationCode != null ? Encoding.UTF8.GetString(c.AuthorizationCode) : string.Empty;

            string fullName = string.Empty;
            if (c.FullName != null)
                fullName = $"{c.FullName.GivenName} {c.FullName.FamilyName}".Trim();

            return new AuthCredential
            {
                Provider          = AuthProvider.Apple,
                UserId            = c.User,
                Email             = c.Email ?? string.Empty,
                DisplayName       = fullName,
                IdToken           = idToken,
                AuthorizationCode = authCode,
                ImageUrl          = string.Empty
            };
        }
    }
}
#endif
