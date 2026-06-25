// =============================================================================
// SAMPLE / TEMPLATE — shows how an app turns AuthKit's AuthCredential into its own
// flow: link the token to a backend, persist the account, map to a project enum.
// This is the ONLY game-specific glue; AuthKit itself never does any of it.
//
// References app-specific types (UserDataServiceManager, PlayerDataMaster, ...) that
// won't exist in a fresh project, so it is guarded by AUTHKIT_SAMPLES and stays inert
// by default. Read it as a reference; to compile, add AUTHKIT_SAMPLES + enable the
// providers you need (AUTHKIT_APPLE / AUTHKIT_GOOGLE) and adapt the type names.
// =============================================================================
#if AUTHKIT_SAMPLES && (AUTHKIT_APPLE || AUTHKIT_GOOGLE)
using System;
using AuthKit;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Roxane.Auth
{
    // Project-specific result of linking a signed-in account to the backend.
    public enum LinkAccountState { Fail = 0, Connected = 1, AlreadyConnected = 2, Conflict = 3 }

    /// <summary>
    /// One shared entry point the UI calls. AuthKit does the native sign-in; this adapter
    /// does the Roxane-specific part (LinkApple/LinkGoogle + persist via PlayerDataMaster).
    /// </summary>
    public sealed class RoxaneAuth
    {
#if AUTHKIT_APPLE
        private readonly ISignInProvider _apple = new AppleSignInProvider();
#endif
#if AUTHKIT_GOOGLE
        private readonly ISignInProvider _google = new GoogleSignInProvider(new GoogleSignInOptions
        {
            WebClientId = "YOUR_WEB_CLIENT_ID.apps.googleusercontent.com",
        });
#endif

        public async UniTask<LinkAccountState> LoginAppleAsync()
        {
#if AUTHKIT_APPLE
            return await LinkAsync(_apple);
#else
            return LinkAccountState.Fail;
#endif
        }

        public async UniTask<LinkAccountState> LoginGoogleAsync()
        {
#if AUTHKIT_GOOGLE
            return await LinkAsync(_google);
#else
            return LinkAccountState.Fail;
#endif
        }

        private async UniTask<LinkAccountState> LinkAsync(ISignInProvider provider)
        {
            AuthResult result = await provider.SignInAsync();
            if (!result.IsSuccess)
            {
                if (result.IsCancelled)
                {
                    Debug.Log("[Auth] cancelled by user");
                }
                else
                {
                    Debug.LogError("[Auth] " + result.ErrorMessage);
                }
                return LinkAccountState.Fail;
            }

            AuthCredential cred = result.Credential;

            // ── Roxane-specific from here ──
            // var response = cred.Provider == AuthProvider.Apple
            //     ? await UserDataServiceManager.Instance.LinkApple(cred.IdToken)
            //     : await UserDataServiceManager.Instance.LinkGoogle(cred.IdToken);
            // if (response == null) return LinkAccountState.Fail;
            // switch (response.status)
            // {
            //     case "connected":
            //     case "already_connected":
            //         PlayerDataMaster.Instance.userDataLocal.UserInfoDataModel.subIdToken = cred.UserId;
            //         await PlayerDataMaster.Instance.SaveUserInfoDataModelAsync();
            //         return response.status == "connected"
            //             ? LinkAccountState.Connected : LinkAccountState.AlreadyConnected;
            //     case "conflict": return LinkAccountState.Conflict;
            //     default:         return LinkAccountState.Fail;
            // }

            Debug.Log($"[Auth] signed in: {cred.Provider} {cred.UserId} {cred.Email}");
            return LinkAccountState.Connected;
        }
    }
}
#endif
