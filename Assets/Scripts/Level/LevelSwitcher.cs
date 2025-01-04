using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSwitcher : MonoBehaviour
{
	[Header("Visual Cue")]
	[SerializeField] private GameObject visualCue;
	public Animator anim;
	[Header("Wybór poziomu")]
	[SerializeField] private string scene;
	private bool isInRange = false;

	private void Update()
	{
		if (isInRange == true)
		{
			if (Input.GetKeyUp(KeyCode.E))
			{
				StartCoroutine(LoadLvl());
			}
		}
	}
	IEnumerator LoadLvl()
	{
		anim.SetTrigger("Start");

		yield return new WaitForSeconds(1);

		SceneManager.LoadScene(scene);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		visualCue.SetActive(true);
		isInRange = true;
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		visualCue.SetActive(false);
		isInRange = false;
	}
}
