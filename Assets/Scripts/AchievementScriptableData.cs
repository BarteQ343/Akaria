using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievementScriptableData", menuName = "ScriptableObjects/AchievementScriptableData")]
public class AchievementScriptableData : ScriptableObject
{
    [SerializeField] private AchievementId id = AchievementId.Unknown;
    [SerializeField] private string displayedName = string.Empty;
    [SerializeField] private string description = String.Empty;
    [SerializeField] private Sprite icon = null;
    [SerializeField] private bool isHidden = false;
    [SerializeField] private bool isUnlocked = false;

    public AchievementId GetId() => id;
    public string GetDisplayName() => displayedName;
    public string GetDescription() => description;
    public Sprite GetIcon() => icon;
    public bool IsHidden() => isHidden;
    public bool IsUnlocked() => isUnlocked;

    public bool TryToUnlockAchievement()
    {
        if (isUnlocked == false)
        {
            isUnlocked = true;
            return true;
        }

        return false;
    }

    public void LockAchievement() => isUnlocked = false;
}