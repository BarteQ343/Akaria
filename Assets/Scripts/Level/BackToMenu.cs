using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
	public Animator anim;
	public CanvasGroup spriteToFade;
	public void BackToMain()
	{
		StartCoroutine(LoadMain());
	}

	IEnumerator LoadMain()
	{
		anim.SetTrigger("Start");

		yield return new WaitForSeconds(1);

		SceneManager.LoadScene("Main Menu");
	}
}
