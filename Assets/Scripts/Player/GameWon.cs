using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWon : MonoBehaviour
{
	[SerializeField]
	private BoxCollider2D GameEnder;
	[SerializeField]
	private GameObject EndScreen;
	[SerializeField]
	private Animator EndBackground;
	private Rigidbody2D r2d;

	private void Start()
	{
		r2d = GetComponent<Rigidbody2D>();
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<BoxCollider2D>() == GameEnder)
		{
			if(GetComponent<PlayerHealthController>().getHasDied() == false)
			{
				AchievementManager.TryToUnlockAchievement(AchievementId.AchievementNoDie);
			}
			if (GameObject.FindWithTag("AchievementMgr").GetComponent<EnemyAchievementManager>().GetHasKilled() == false)
			{
				AchievementManager.TryToUnlockAchievement(AchievementId.AchievementPacifist);
			}
			if (GetComponent<PlayerHealthController>().GetHealth() <= 10)
			{
				AchievementManager.TryToUnlockAchievement(AchievementId.AchievementLowHP);
			}
			r2d.constraints = RigidbodyConstraints2D.FreezeAll;
			ShowEndScreen();
		}
	}
	public void ShowEndScreen()
	{
		EndScreen.SetActive(true);
		EndBackground.ResetTrigger("End");
		EndBackground.SetTrigger("Start");
		Cursor.visible = true;
	}
}
