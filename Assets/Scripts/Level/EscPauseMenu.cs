using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscPauseMenu : MonoBehaviour
{
	[SerializeField] GameObject pauseMenu;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (pauseMenu.activeSelf == false)
			{
				pauseMenu.SetActive(true);
				Time.timeScale = 0;
			} else
			{
				pauseMenu.SetActive(false);
				Time.timeScale = 1;
			}
		}
	}
}
