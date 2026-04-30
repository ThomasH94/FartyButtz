using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Registry of all ButtData skins. Extends Database so entries are
/// auto-indexed by ID and name.
///
/// SETUP:
///   1. Right-click > Game > Butt DB  (create one instance)
///   2. Place it at Resources/DB/ButtDB.asset  (Database.Instance loads it automatically)
///   3. Add all ButtData SOs to the entries list
///   4. Set DefaultSkin to the skin all new players receive
///
/// Access anywhere via:  ButtDB.Instance.GetById(id)
///                       ButtDB.Instance.GetByPlayFabId("butt_default")
///                       ButtDB.Instance.GetOwnedSkins(ownedItemIds)
/// </summary>
[CreateAssetMenu(menuName = "Game/Butt DB")]
public class ButtDB : Database<ButtDB, ButtData>
{
    [Header("Defaults")]
    [Tooltip("Granted to every new player. Must also exist in PlayFab catalog.")]
    public ButtData DefaultSkin;

    // Secondary index: PlayFab Item ID -> ButtData (built alongside base index)
    private Dictionary<string, ButtData> _byPlayFabId;

    protected override void BuildIndex()
    {
        base.BuildIndex();

        _byPlayFabId = new Dictionary<string, ButtData>();
        foreach (var entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.PlayFabItemId)) continue;
            if (!_byPlayFabId.TryAdd(entry.PlayFabItemId, entry))
                Debug.LogWarning($"[ButtDB] Duplicate PlayFabItemId: {entry.PlayFabItemId}");
        }
    }

    /// <summary>Returns the ButtData whose PlayFabItemId matches, or null.</summary>
    public ButtData GetByPlayFabId(string playFabItemId)
    {
        if (_byPlayFabId == null) BuildIndex();
        _byPlayFabId.TryGetValue(playFabItemId, out var result);
        return result;
    }

    /// <summary>Returns all skins the player currently owns based on their PlayFab inventory.</summary>
    public List<ButtData> GetOwnedSkins(IEnumerable<string> ownedItemIds)
    {
        if (_byPlayFabId == null) BuildIndex();
        return ownedItemIds
            .Select(GetByPlayFabId)
            .Where(skin => skin != null)
            .ToList();
    }

    /// <summary>Returns true if the player owns this skin.</summary>
    public bool IsOwned(ButtData skin, IEnumerable<string> ownedItemIds)
        => ownedItemIds.Contains(skin.PlayFabItemId);

#if UNITY_EDITOR
    [Button("Validate DB")]
    private void ValidateDB()
    {
        int issues = 0;

        if (DefaultSkin == null)
        { Debug.LogError("[ButtDB] No DefaultSkin assigned!"); issues++; }

        foreach (var skin in entries)
        {
            if (skin == null)
            { Debug.LogError("[ButtDB] Null entry in list."); issues++; continue; }

            if (string.IsNullOrEmpty(skin.PlayFabItemId))
            { Debug.LogError($"[ButtDB] {skin.name} has no PlayFabItemId!"); issues++; }

            if (skin.ButtSprite == null)
                Debug.LogWarning($"[ButtDB] {skin.name} has no ButtSprite.");
        }

        if (DefaultSkin != null && !entries.Contains(DefaultSkin))
        { Debug.LogError("[ButtDB] DefaultSkin is not in the entries list!"); issues++; }

        Debug.Log(issues == 0
            ? $"[ButtDB] Valid — {entries.Count} skins."
            : $"[ButtDB] {issues} issue(s) found.");
    }
#endif
}