using System.Collections.Generic;
using UnityEngine;

public class EnemyAchievementManager : MonoBehaviour
{
    public int enemyCounter;
    public bool hasKilled; 
    public List<string> enemyTypes = new List<string>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyCounter = GameObject.FindGameObjectsWithTag("Enemy").Length + GameObject.FindGameObjectsWithTag("Ghoul").Length;
        enemyTypes.Add("Enemy"); enemyTypes.Add("Ghoul");
    }
    private void Update()
    {
        if (enemyTypes.Count == 0)
        {
            AchievementManager.TryToUnlockAchievement(AchievementId.AchievementMonsterHunted);
        }
    }
    // Update is called once per frame
    public void UpdateVars(string Tag)
    {
        if (enemyCounter > 0)
        {
            enemyCounter -= 1;
            hasKilled = true;
            enemyTypes.Remove(Tag);
        }
        else
        {
            AchievementManager.TryToUnlockAchievement(AchievementId.AchievementKiller);
        }
    }
    public bool GetHasKilled()
    {
        return hasKilled;
    }
}
