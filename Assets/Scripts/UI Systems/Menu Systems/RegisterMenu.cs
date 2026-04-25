using UnityEngine;
using TMPro;

public class RegisterMenuData : IMenuData
{
    // Could pre-fill email if navigating from LoginMenu with a typed address
    public string PrefilledEmail { get; }
    public RegisterMenuData(string prefilledEmail = "") => PrefilledEmail = prefilledEmail;
}

/// <summary>
/// New account registration screen.
///
/// Fields: Display Name, Email, Password, Confirm Password
///
/// Flow:
///   Validate -> AuthManager.RegisterWithEmail()
///   -> On PlayFabRegisterSuccessPayload: show brief success state
///   -> AuthManager auto-logs in after registration
///   -> GameManager reacts to PlayFabLoginSuccessPayload, opens MainMenu
///
/// On failure: show inline error and re-enable form.
/// </summary>
public class RegisterMenu : BaseMenu
{
    [Header("Fields")]
    [SerializeField] private TMP_InputField m_DisplayNameInput = null;
    [SerializeField] private TMP_InputField m_EmailInput = null;
    [SerializeField] private TMP_InputField m_PasswordInput = null;
    [SerializeField] private TMP_InputField m_ConfirmPasswordInput = null;

    [Header("Feedback")]
    [SerializeField] private GameObject m_LoadingIndicator = null;
    [SerializeField] private TMP_Text m_ErrorText = null;

    [Header("Buttons")]
    [SerializeField] private ExtendedButton m_RegisterButton = null;
    [SerializeField] private ExtendedButton m_BackButton = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_RegisterButton.RegisterClickAction(OnRegisterClicked);
        m_BackButton.RegisterClickAction(OnBackClicked);

        EventBus.Subscribe<PlayFabRegisterSuccessPayload>(OnRegisterSuccess);
        EventBus.Subscribe<PlayFabRegisterFailedPayload>(OnRegisterFailed);

        // Pre-fill email if passed via MenuData
        if (MenuData is RegisterMenuData data && !string.IsNullOrEmpty(data.PrefilledEmail))
            m_EmailInput.text = data.PrefilledEmail;

        SetLoading(false);
        ClearError();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus.Unsubscribe<PlayFabRegisterSuccessPayload>(OnRegisterSuccess);
        EventBus.Unsubscribe<PlayFabRegisterFailedPayload>(OnRegisterFailed);
    }

    // -------------------------------------------------------------------------
    // BUTTON HANDLERS
    // -------------------------------------------------------------------------
    private void OnRegisterClicked()
    {
        ClearError();

        string displayName      = m_DisplayNameInput.text.Trim();
        string email            = m_EmailInput.text.Trim();
        string password         = m_PasswordInput.text;
        string confirmPassword  = m_ConfirmPasswordInput.text;

        if (!ValidateFields(displayName, email, password, confirmPassword)) return;

        SetLoading(true);
        AuthManager.Instance.RegisterWithEmail(email, password, displayName);
    }

    private void OnBackClicked()
    {
        EventBus.Publish(new MenuRequestOpenPayload(typeof(LoginMenu), null));
    }

    // -------------------------------------------------------------------------
    // EVENT HANDLERS
    // -------------------------------------------------------------------------
    private void OnRegisterSuccess(PlayFabRegisterSuccessPayload payload)
    {
        // AuthManager auto-logs in after registration.
        // GameManager will pick up PlayFabLoginSuccessPayload and open MainMenu.
        // Nothing to do here except keep the loading state up so it feels intentional.
        Debug.Log("[RegisterMenu] Registration successful — awaiting auto-login.");
    }

    private void OnRegisterFailed(PlayFabRegisterFailedPayload payload)
    {
        SetLoading(false);
        ShowError(FriendlyError(payload.ErrorMessage));
    }

    // -------------------------------------------------------------------------
    // VALIDATION
    // -------------------------------------------------------------------------
    private bool ValidateFields(string displayName, string email, string password, string confirm)
    {
        if (string.IsNullOrEmpty(displayName))
        {
            ShowError("Please enter a display name.");
            return false;
        }

        if (displayName.Length < 3 || displayName.Length > 25)
        {
            ShowError("Display name must be between 3 and 25 characters.");
            return false;
        }

        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            ShowError("Please enter a valid email address.");
            return false;
        }

        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            ShowError("Password must be at least 6 characters.");
            return false;
        }

        if (password != confirm)
        {
            ShowError("Passwords do not match.");
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

        m_RegisterButton.interactable = !loading;
        m_BackButton.interactable     = !loading;
    }

    private void ShowError(string message)
    {
        if (m_ErrorText == null) return;

        // Deactivate first to force TMP to re-layout even if already visible
        m_ErrorText.gameObject.SetActive(false);
        m_ErrorText.text = message;
        m_ErrorText.gameObject.SetActive(true);
    }

    private void ClearError()
    {
        if (m_ErrorText != null)
            m_ErrorText.gameObject.SetActive(false);
    }

    private static string FriendlyError(string raw)
    {
        if (raw.Contains("EmailAddressNotAvailable") || raw.Contains("EmailAddressAlreadyExists"))
            return "That email address is already registered.";
        if (raw.Contains("InvalidEmailAddress"))
            return "Please enter a valid email address.";
        if (raw.Contains("InvalidPassword"))
            return "Password must be at least 6 characters and contain letters and numbers.";
        if (raw.Contains("NameNotAvailable") || raw.Contains("DisplayNameNotAvailable"))
            return "That display name is already taken. Please try another.";
        if (raw.Contains("connection") || raw.Contains("timeout"))
            return "Connection failed. Please check your internet.";

        // Log the raw error in dev builds so you can catch any unmapped codes
        Debug.LogWarning($"[RegisterMenu] Unmapped error: {raw}");
        return "Registration failed. Please try again.";
    }
}