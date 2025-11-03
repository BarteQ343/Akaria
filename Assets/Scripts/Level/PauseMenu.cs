using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
	[SerializeField] GameObject pauseMenu;
	[Header("Do obu podpinacie obiekt Image z LevelLoader'a")]
	public Animator anim;
	public CanvasGroup spriteToFade;

	public void Resume()
	{
		pauseMenu.SetActive(false);
		Time.timeScale = 1;
	}

	public void SaveGame()
	{
		Debug.Log("Saved! \n(not actually, it does nothing yet)");
	}
	public void OpenSettings()
	{
		Debug.Log("Here are the settings! \n(not actually, it does nothing yet)");
	}

	public void BackToMenu()
	{
		StartCoroutine(LoadMain());
	}

	IEnumerator LoadMain()
	{
		Time.timeScale = 1;

		anim.SetTrigger("Start");

		yield return new WaitForSeconds(1);

		SceneManager.LoadScene("Main Menu");
	}
}
