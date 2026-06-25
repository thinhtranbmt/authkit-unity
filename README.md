# AuthKit

Reusable native **sign-in** for Unity — **Sign in with Apple** and **Google Sign-In** — behind
one small interface. No singleton: plain-class providers that return a provider-neutral
`AuthCredential` via UniTask. Linking the token to your backend and persisting the account
stay in **your** app. Mirrors the `HttpKit` / `IAPKit` convention.

## Design

- **`ISignInProvider`** — `SignInAsync()` → `AuthResult` (Success / Cancelled / Failed),
  `IsSupported`, `SignOut()`. The app can depend on just this interface.
- **`AuthCredential`** — provider-neutral: `UserId`, `Email`, `DisplayName`, `IdToken`,
  `AuthorizationCode` (Apple), `ImageUrl` (Google).
- **Providers are separate, define-gated assemblies** — `MyCore.AuthKit.Apple`
  (`AUTHKIT_APPLE`) and `MyCore.AuthKit.Google` (`AUTHKIT_GOOGLE`). Each is excluded from
  compilation entirely unless its define is set, so a project that uses only one — or
  neither — never needs the other SDK. The core has **zero SDK dependency**.

## Requirements

| Provider | Enable | SDK to install |
|---|---|---|
| Core | — | UniTask (required, install separately) |
| Apple | define `AUTHKIT_APPLE` | Apple Sign-In Unity plugin (asmdef `AppleAuth`) |
| Google | define `AUTHKIT_GOOGLE` | `com.google.signin` (Google Sign-In Unity plugin) |

Neither Apple nor Google SDK is on the Unity registry, so AuthKit can't auto-resolve them —
install the one(s) you need and add the matching define.

## Install

1. **Install UniTask** (not on the registry):
   ```json
   "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
   ```
2. **Install AuthKit:**
   ```json
   "com.mycore.authkit": "https://github.com/thinhtranbmt/authkit-unity.git#v0.1.0"
   ```
3. **Install the provider SDK(s)** you want and add the define(s):
   - Apple → install the Apple Sign-In plugin, add `AUTHKIT_APPLE` to *Project Settings ▸ Player ▸ Scripting Define Symbols*.
   - Google → `"com.google.signin": "https://github.com/Thaina/google-signin-unity.git#newmigration"`, add `AUTHKIT_GOOGLE`.

## Usage

```csharp
using AuthKit;

// Apple
ISignInProvider apple = new AppleSignInProvider();           // needs AUTHKIT_APPLE
if (apple.IsSupported)
{
    AuthResult r = await apple.SignInAsync();
    if (r.IsSuccess) PostToBackend(r.Credential.IdToken);    // your call
    else if (r.IsCancelled) { /* user dismissed */ }
    else Debug.LogError(r.ErrorMessage);
}

// Google
ISignInProvider google = new GoogleSignInProvider(new GoogleSignInOptions  // needs AUTHKIT_GOOGLE
{
    WebClientId = "....apps.googleusercontent.com",
});
AuthResult g = await google.SignInAsync();
```

`AuthCredential` gives you `IdToken` (POST to your backend), `UserId`, `Email`, `DisplayName`,
plus `AuthorizationCode` (Apple) / `ImageUrl` (Google).

## What's NOT in the Kit (stays in your app)
Linking the token to a backend, mapping the response to your own enum, persisting the account,
and any UI. See the **App Auth Adapter** sample for a template.

## Notes
- Apple requires no scene object — the Kit pumps `AppleAuthManager.Update()` via UniTask only
  during the sign-in call.
- Verify on **device** (Apple sign-in does not work in the Editor; Google needs a real build
  for the native flow).

## License
See [LICENSE.md](LICENSE.md).
