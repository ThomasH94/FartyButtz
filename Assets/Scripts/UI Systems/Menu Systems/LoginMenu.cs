using UnityEngine;
using TMPro;

public class LoginMenuData : IMenuData
{
    // Reserved for passing context into the login menu if needed in future
    // (e.g. a "session expired" flag to show a message on re-open)
}

/// <summary>
/// Handles the player-facing login screen.
///
/// Five paths:
///   Guest        -> AuthManager.LoginAsGuest()
///   Email        -> Validate fields -> AuthManager.LoginWithEmail()
///   Google       -> PlatformAuthHelper.SignInWithGoogle() -> AuthManager.LoginWithGoogle()
///   Apple        -> PlatformAuthHelper.SignInWithApple()  -> AuthManager.LoginWithApple()
///   Register     -> Open RegisterMenu
///
/// Platform buttons are shown/hidden automatically based on the current platform:
///   Android  -> Google button visible, Apple button hidden
///   iOS      -> Apple button visible, Google button hidden
///   Editor   -> Both visible for testing
///
/// GameManager handles all post-login data loading — LoginMenu only drives auth.
/// </summary>
public class LoginMenu : BaseMenu
{
    [Header("Email Login Fields")]
    [SerializeField] private TMP_InputField m_EmailInput = null;
    [SerializeField] private TMP_InputField m_PasswordInput = null;

    [Header("Feedback")]
    [SerializeField] private GameObject m_LoadingIndicator = null;
    [SerializeField] private TMP_Text m_ErrorText = null;

    [Header("Buttons")]
    [SerializeField] private ExtendedButton m_LoginButton = null;
    [SerializeField] private ExtendedButton m_GuestButton = null;
    [SerializeField] private ExtendedButton m_GoogleButton = null;
    [SerializeField] private ExtendedButton m_AppleButton = null;
    [SerializeField] private ExtendedButton m_RegisterAccountButton = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_LoginButton.RegisterClickAction(OnLoginClicked);
        m_GuestButton.RegisterClickAction(OnGuestClicked);
        m_GoogleButton.RegisterClickAction(OnGoogleClicked);
        m_AppleButton.RegisterClickAction(OnAppleClicked);
        m_RegisterAccountButton.RegisterClickAction(OnRegisterNewAccountClicked);

        EventBus.Subscribe<PlayFabLoginFailedPayload>(OnLoginFailed);

        ConfigurePlatformButtons();
        SetLoading(false);
        ClearError();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus.Unsubscribe<PlayFabLoginFailedPayload>(OnLoginFailed);
    }

    // -------------------------------------------------------------------------
    // PLATFORM BUTTON VISIBILITY
    // Show the right sign-in option for the current platform.
    // -------------------------------------------------------------------------
    private void ConfigurePlatformButtons()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_GoogleButton.gameObject.SetActive(true);
        m_AppleButton.gameObject.SetActive(false);
#elif UNITY_IOS && !UNITY_EDITOR
        m_GoogleButton.gameObject.SetActive(false);
        m_AppleButton.gameObject.SetActive(true);
#else
        // Editor: show both so you can test the UI layout
        m_GoogleButton.gameObject.SetActive(true);
        m_AppleButton.gameObject.SetActive(true);
#endif
    }

    // -------------------------------------------------------------------------
    // BUTTON HANDLERS
    // -------------------------------------------------------------------------
    private void OnGuestClicked()
    {
        ClearError();
        SetLoading(true);
        AuthManager.Instance.LoginAsGuest();
    }

    private void OnLoginClicked()
    {
        ClearError();

        string email    = m_EmailInput.text.Trim();
        string password = m_PasswordInput.text;

        if (!ValidateEmailFields(email, password)) return;

        SetLoading(true);
        AuthManager.Instance.LoginWithEmail(email, password);
    }

    private void OnGoogleClicked()
    {
        ClearError();
        SetLoading(true);

        // PlatformAuthHelper abstracts the Google Play Games plugin.
        // It calls AuthManager.Instance.LoginWithGoogle() internally on success,
        // or publishes PlayFabLoginFailedPayload on failure.
        PlatformAuthHelper.SignInWithGoogle();
    }

    private void OnAppleClicked()
    {
        ClearError();
        SetLoading(true);

        // PlatformAuthHelper abstracts the Apple Sign In plugin.
        // It calls AuthManager.Instance.LoginWithApple() internally on success,
        // or publishes PlayFabLoginFailedPayload on failure.
        PlatformAuthHelper.SignInWithApple();
    }

    private void OnRegisterNewAccountClicked()
    {
        EventBus.Publish(new MenuRequestOpenPayload(typeof(RegisterMenu), null, true));
    }

    // -------------------------------------------------------------------------
    // EVENT HANDLERS
    // -------------------------------------------------------------------------

    // Login success is handled by GameManager which loads data and opens MainMenu.
    // LoginMenu doesn't react to success — it just closes when MainMenu opens.

    private void OnLoginFailed(PlayFabLoginFailedPayload payload)
    {
        SetLoading(false);
        ShowError(FriendlyError(payload.ErrorMessage));
    }

    // -------------------------------------------------------------------------
    // VALIDATION
    // -------------------------------------------------------------------------
    private bool ValidateEmailFields(string email, string password)
    {
        if (string.IsNullOrEmpty(email))
        {
            ShowError("Please enter your email address.");
            return false;
        }

        if (!email.Contains("@"))
        {
            ShowError("Please enter a valid email address.");
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowError("Please enter your password.");
            return false;
        }

        if (password.Length < 6)
        {
            ShowError("Password must be at least 6 characters.");
            return false;
        }

        return true;
    }

    // -------------------------------------------------------------------------
    // UI HELPERS
    // -------------------------------------------------------------------------
    private void SetLoading(bool loading)
    {
        if (m_LoadingIndicator != null)
            m_LoadingIndicator.SetActive(loading);

        m_LoginButton.interactable           = !loading;
        m_GuestButton.interactable           = !loading;
        m_GoogleButton.interactable          = !loading;
        m_AppleButton.interactable           = !loading;
        m_RegisterAccountButton.interactable = !loading;
    }

    private void ShowError(string message)
    {
        if (m_ErrorText != null)
        {
            m_ErrorText.text = message;
            m_ErrorText.gameObject.SetActive(true);
        }
    }

    private void ClearError()
    {
        if (m_ErrorText != null)
            m_ErrorText.gameObject.SetActive(false);
    }

    /// <summary>Turns PlayFab's raw error strings into player-friendly messages.</summary>
    private static string FriendlyError(string raw)
    {
        if (raw.Contains("InvalidEmailOrPassword") || raw.Contains("AccountNotFound"))
            return "Incorrect email or password.";
        if (raw.Contains("EmailAddressNotAvailable"))
            return "That email is already in use.";
        if (raw.Contains("connection") || raw.Contains("timeout"))
            return "Connection failed. Please check your internet.";
        return "Something went wrong. Please try again.";
    }
}