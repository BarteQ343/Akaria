using System;
using UnityEngine;
using UnityEngine.Events;

public class AchievementManager : MonoBehaviour
{
    public static event UnityAction<AchievementScriptableData> OnAchievementUnlocked = null;

    private static AchievementManager instance = null;

    [SerializeField] private AchievementScriptableData[] achievements = Array.Empty<AchievementScriptableData>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TryToUnlockAchievement(AchievementScriptableData _achievement)
    {
        if (instance == null) return;

        if (_achievement.TryToUnlockAchievement())
        {
            OnAchievementUnlocked?.Invoke(_achievement);
        }
    }

    private AchievementScriptableData getAchievement(AchievementId _id)
    {
        foreach (AchievementScriptableData achievement in achievements)
        {
            if (achievement.GetId() == _id) return achievement;
        }

        return null;
    }

    public static void TryToUnlockAchievement(AchievementId _id)
    {
        if (instance == null) return;

        if (instance.getAchievement(_id).TryToUnlockAchievement())
        {
            OnAchievementUnlocked?.Invoke(instance.getAchievement(_id));
        }
    }

    public static void LockAllAchievements()
    {
        if (instance == null) return;

        foreach (AchievementScriptableData achievement in instance.achievements)
        {
            achievement.LockAchievement();
        }
    }

    public static AchievementScriptableData[] GetAchievements()
    {
        return instance == null ? null : instance.achievements;
    }
}