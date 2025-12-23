using UnityEngine;

public class ExplorerAchievementHandler : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Collider2D>() == GameObject.FindGameObjectWithTag("Player"))
        {
            AchievementManager.TryToUnlockAchievement(AchievementId.AchievementExplorer);
        }
    }
}
