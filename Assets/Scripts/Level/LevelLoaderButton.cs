using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoaderButton : MonoBehaviour
{
	public string scene;
	public Animator anim;
	public CanvasGroup spriteToFade;
	int counter = 0;
	private void Update()
	{
		if (spriteToFade.alpha == 1 && counter > 0)
		{
			SceneManager.LoadScene(scene);
		}
	}
	public void LoadFirstLevel()
	{
		counter++;
		anim.SetTrigger("Start");
	}
}
