using Cysharp.Threading.Tasks;

namespace AuthKit
{
    // =====================================================================
    // AuthKit — reusable native sign-in (Apple / Google) for Unity.
    //
    // The Kit ONLY performs the native sign-in and returns an AuthCredential
    // (userId, email, displayName, idToken, ...). What happens next — linking the
    // token to your backend, persisting the account, mapping to your own enums —
    // is GAME-SPECIFIC and stays in the app (see Samples~).
    //
    // Each provider lives in its own assembly gated by a define constraint
    // (AUTHKIT_APPLE / AUTHKIT_GOOGLE), so a project that uses only one — or
    // neither — never needs the other SDK. The core here has zero SDK dependency.
    // Only hard dependency: UniTask.
    // =====================================================================

    public enum AuthProvider
    {
        Apple,
        Google
    }

    public enum AuthOutcome
    {
        Success,
        Cancelled,   // user dismissed the native dialog
        Failed       // error / unsupported platform
    }

    /// <summary>Provider-neutral result of a native sign-in. App maps this to its own model.</summary>
    public sealed class AuthCredential
    {
        public AuthProvider Provider;
        public string UserId;
        public string Email;
        public string DisplayName;
        public string IdToken;             // the token you POST to your backend
        public string AuthorizationCode;   // Apple only (empty otherwise)
        public string ImageUrl;            // Google only (empty otherwise)
    }

    /// <summary>Rich result — never throws on the happy path; inspect Outcome.</summary>
    public sealed class AuthResult
    {
        public AuthOutcome Outcome;
        public AuthCredential Credential;
        public string ErrorMessage;

        public bool IsSuccess => Outcome == AuthOutcome.Success;
        public bool IsCancelled => Outcome == AuthOutcome.Cancelled;

        public static AuthResult Success(AuthCredential credential)
            => new AuthResult { Outcome = AuthOutcome.Success, Credential = credential };

        public static AuthResult Cancelled()
            => new AuthResult { Outcome = AuthOutcome.Cancelled };

        public static AuthResult Failed(string message)
            => new AuthResult { Outcome = AuthOutcome.Failed, ErrorMessage = message };
    }

    /// <summary>
    /// A native sign-in provider. Implemented by AppleSignInProvider / GoogleSignInProvider
    /// (each in its own define-gated assembly). The app can depend on this interface and
    /// stay agnostic about which providers are compiled in.
    /// </summary>
    public interface ISignInProvider
    {
        AuthProvider Provider { get; }

        /// <summary>True when sign-in can run on the current platform/build.</summary>
        bool IsSupported { get; }

        /// <summary>Runs the native sign-in flow. Resolves to Success / Cancelled / Failed.</summary>
        UniTask<AuthResult> SignInAsync();

        /// <summary>Signs out / clears the cached session (no-op for Apple).</summary>
        void SignOut();
    }
}
