using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
	public string scene;
	public Animator anim;
	public CanvasGroup spriteToFade;
	int counter = 0;
	void OnTriggerEnter2D(Collider2D other)
	{
		LoadFirstLevel();
		if (spriteToFade.alpha == 1 && counter > 0)
		{
			SceneManager.LoadScene(scene);
		}
	}
	public void LoadFirstLevel()
	{
		counter++;
		anim.SetTrigger("Start");
		Cursor.visible = false;
	}
}
