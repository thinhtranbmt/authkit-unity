# Changelog

All notable changes to this package are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/), and this project adheres to
[Semantic Versioning](https://semver.org/).

## [0.1.0] - 2026-06-25
### Added
- Initial release.
- `ISignInProvider` + provider-neutral `AuthCredential` / `AuthResult` (Success / Cancelled / Failed).
- `AppleSignInProvider` — Sign in with Apple, plain class (pumps `AppleAuthManager.Update()` via UniTask, no MonoBehaviour). Gated by `AUTHKIT_APPLE`.
- `GoogleSignInProvider` (+ `GoogleSignInOptions`) — Google Sign-In, plain class, WebClientId injected. Gated by `AUTHKIT_GOOGLE`.
- Each provider in its own define-gated assembly; core has zero SDK dependency.
- App Auth Adapter sample (template) — backend link + persistence (guarded by `AUTHKIT_SAMPLES`).
