using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleOptionsInMain : MonoBehaviour
{
	public GameObject MainMenu;
	public GameObject OptionsMenu;

	public void ToggleOptions()
	{
		if (MainMenu.activeSelf == true)
		{
			MainMenu.SetActive(false);
			OptionsMenu.SetActive(true);
		}
		else
		{
			MainMenu.SetActive(true);
			OptionsMenu.SetActive(false);
		}
	}
}
