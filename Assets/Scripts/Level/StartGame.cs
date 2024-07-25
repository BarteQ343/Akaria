using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
	public Animator anim;
	public CanvasGroup spriteToFade;
	int counter = 0;
	void Update()
	{
		if (spriteToFade.alpha == 1 && counter > 0)
		{
			SceneManager.LoadScene("StartingPoint");
		}
	}
	public void LoadFirstLevel()
	{
		counter++;
		anim.SetTrigger("Start");
		Cursor.visible = false;
	}
}
