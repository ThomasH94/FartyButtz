using UnityEngine;
using System;

/// <summary>
/// Owns all session persistence. Stores a randomly-generated GUID in PlayerPrefs
/// as a silent re-login key. No credentials, no tokens — just an opaque ID that
/// PlayFab's LoginWithCustomID validates server-side.
///
/// Flow on launch:
///   HasSavedSession() true  -> AuthManager.LoginWithSavedSession() -> silent auto-login
///   HasSavedSession() false -> show LoginMenu
///
/// Flow on successful login (any method):
///   GameManager calls SaveSession() to persist the GUID for next launch.
///
/// Flow on logout or auth failure:
///   ClearSession() wipes PlayerPrefs so next launch shows LoginMenu.
///
/// Why GUID and not device ID?
///   Device ID is hardware-tied and can't be invalidated. A GUID lets us call
///   ClearSession() to force re-login (e.g. after logout or account switch)
///   while still surviving app reinstalls IF the OS preserves PlayerPrefs (iOS does,
///   Android does not after a clean uninstall). For cross-device persistence the
///   player must use email or platform login — guest sessions are device-local.
/// </summary>
public class SessionManager : SingletonMonoBehaviour<SessionManager>
{
    private const string KEY_SESSION_GUID    = "pf_session_guid";
    private const string KEY_DISPLAY_NAME    = "pf_display_name";
    private const string KEY_LOGIN_TYPE      = "pf_login_type";   // "guest" | "email" | "google" | "apple"

    public string SavedDisplayName => PlayerPrefs.GetString(KEY_DISPLAY_NAME, string.Empty);
    public string SavedLoginType   => PlayerPrefs.GetString(KEY_LOGIN_TYPE,   "guest");

    protected override void Awake() => base.Awake();

    // -------------------------------------------------------------------------
    // READ
    // -------------------------------------------------------------------------

    /// <summary>Returns true if a re-login GUID exists from a previous session.</summary>
    public bool HasSavedSession() => PlayerPrefs.HasKey(KEY_SESSION_GUID);

    /// <summary>Returns the stored GUID, or null if none exists.</summary>
    public string GetSessionGuid() => PlayerPrefs.GetString(KEY_SESSION_GUID, null);

    // -------------------------------------------------------------------------
    // WRITE — call after any successful login
    // -------------------------------------------------------------------------

    /// <summary>
    /// Persists a GUID for silent re-login on next launch.
    /// Pass the GUID that was used for LoginWithCustomID, or generate a fresh one
    /// for email/platform logins so they also get silent re-login.
    /// </summary>
    public void SaveSession(string guid, string displayName, string loginType)
    {
        PlayerPrefs.SetString(KEY_SESSION_GUID,  guid);
        PlayerPrefs.SetString(KEY_DISPLAY_NAME,  displayName);
        PlayerPrefs.SetString(KEY_LOGIN_TYPE,    loginType);
        PlayerPrefs.Save();
        Debug.Log($"[Session] Saved session — type={loginType}, name={displayName}");
    }

    /// <summary>
    /// Generates and saves a new GUID. Use for email/platform logins so they
    /// also benefit from silent re-login without storing credentials.
    /// Returns the generated GUID so AuthManager can link it to the PlayFab account.
    /// </summary>
    public string GenerateAndSaveSession(string displayName, string loginType)
    {
        string guid = Guid.NewGuid().ToString();
        SaveSession(guid, displayName, loginType);
        return guid;
    }

    // -------------------------------------------------------------------------
    // CLEAR — call on logout or when silent login fails
    // -------------------------------------------------------------------------

    public void ClearSession()
    {
        PlayerPrefs.DeleteKey(KEY_SESSION_GUID);
        PlayerPrefs.DeleteKey(KEY_DISPLAY_NAME);
        PlayerPrefs.DeleteKey(KEY_LOGIN_TYPE);
        PlayerPrefs.Save();
        Debug.Log("[Session] Session cleared.");
    }
}
