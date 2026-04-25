using UnityEngine;

/// <summary>
/// Abstracts platform-specific sign-in SDKs so LoginMenu stays clean.
///
/// SETUP REQUIRED:
///   Android: Import Google Play Games plugin for Unity
///            https://github.com/playgameservices/play-games-plugin-for-unity
///            Enable in Project Settings > Google Play Games
///
///   iOS:     Import Apple Sign In Unity plugin
///            https://github.com/lupidan/apple-signin-unity
///
/// Both paths end by calling AuthManager, which publishes the result to the EventBus.
/// On failure, we publish PlayFabLoginFailedPayload directly so LoginMenu
/// can show the error without needing to know which SDK failed.
/// </summary>
public static class PlatformAuthHelper
{
#if UNITY_ANDROID
    // Uncomment and configure once the Google Play Games plugin is imported:
    // private static bool s_GoogleInitialized = false;
    //
    // public static void InitializeGoogle()
    // {
    //     var config = new GooglePlayGames.BasicApi.PlayGamesClientConfiguration.Builder()
    //         .RequestServerAuthCode(false)
    //         .Build();
    //     GooglePlayGames.PlayGamesPlatform.InitializeInstance(config);
    //     GooglePlayGames.PlayGamesPlatform.Activate();
    //     s_GoogleInitialized = true;
    // }
#endif

    // -------------------------------------------------------------------------
    // GOOGLE SIGN-IN (Android)
    // -------------------------------------------------------------------------
    public static void SignInWithGoogle()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Requires Google Play Games plugin
        // Social.localUser.Authenticate(success =>
        // {
        //     if (!success)
        //     {
        //         EventBus.Publish(new PlayFabLoginFailedPayload("Google sign-in was cancelled or failed."));
        //         return;
        //     }
        //     string authCode = GooglePlayGames.PlayGamesPlatform.Instance.GetServerAuthCode();
        //     AuthManager.Instance.LoginWithGoogle(authCode);
        // });

        // ---- TEMPORARY: remove once plugin is set up ----
        Debug.LogWarning("[PlatformAuth] Google Play Games plugin not yet configured.");
        EventBus.Publish(new PlayFabLoginFailedPayload("Google sign-in is not set up yet."));
#else
        // Editor stub — simulates a successful Google login using guest credentials
        Debug.Log("[PlatformAuth] Editor: simulating Google login as guest.");
        AuthManager.Instance.LoginAsGuest();
#endif
    }

    // -------------------------------------------------------------------------
    // APPLE SIGN-IN (iOS)
    // -------------------------------------------------------------------------
    public static void SignInWithApple()
    {
#if UNITY_IOS && !UNITY_EDITOR
        // Requires Apple Sign In Unity plugin (lupidan/apple-signin-unity)
        //
        // var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
        // AppleAuthManager.Instance.LoginWithAppleId(loginArgs,
        //     credential => {
        //         if (credential is IAppleIDCredential appleCredential)
        //         {
        //             string identityToken = Encoding.UTF8.GetString(appleCredential.IdentityToken);
        //             AuthManager.Instance.LoginWithApple(identityToken);
        //         }
        //     },
        //     error => {
        //         var authError = (AuthorizationErrorCode)error.GetAuthorizationErrorCode();
        //         if (authError == AuthorizationErrorCode.Canceled)
        //             EventBus.Publish(new PlayFabLoginFailedPayload("Apple sign-in was cancelled."));
        //         else
        //             EventBus.Publish(new PlayFabLoginFailedPayload("Apple sign-in failed. Please try again."));
        //     }
        // );

        // ---- TEMPORARY: remove once plugin is set up ----
        Debug.LogWarning("[PlatformAuth] Apple Sign In plugin not yet configured.");
        EventBus.Publish(new PlayFabLoginFailedPayload("Apple sign-in is not set up yet."));
#else
        // Editor stub — simulates a successful Apple login using guest credentials
        Debug.Log("[PlatformAuth] Editor: simulating Apple login as guest.");
        AuthManager.Instance.LoginAsGuest();
#endif
    }
}
