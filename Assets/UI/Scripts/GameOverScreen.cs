using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
	public GameObject YouLose;
	public GameObject RespawnButton;
	public GameObject GiveUpButton;
	public Image spriteToFade;
	public IEnumerator fadeIn(float duration)
	{
		float counter = 0;
		//Get current color
		Color spriteColor = Color.black;

		while (counter < duration)
		{
			counter += Time.deltaTime;
			//Fade from 1 to 0
			float alpha = Mathf.Lerp(0, 1, counter / duration);
			Debug.Log(alpha);

			//Change alpha only
			spriteToFade.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, alpha);
			if (spriteToFade.color.a == 1)
			{
				YouLose.SetActive(true);
				RespawnButton.SetActive(true);
				GiveUpButton.SetActive(true);
			}
			//Wait for a frame
			yield return null;
		}
	}
	public IEnumerator fadeOut(float duration)
	{
		float counter = 0;
		//Get current color
		Color spriteColor = Color.black;

		while (counter < duration)
		{
			counter += Time.deltaTime;
			//Fade from 1 to 0
			float alpha = Mathf.Lerp(1, 0, counter / duration);
			Debug.Log(alpha);

			//Change alpha only
			spriteToFade.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, alpha);
			if (spriteToFade.color.a == 0)
			{
				YouLose.SetActive(false);
				RespawnButton.SetActive(false);
				GiveUpButton.SetActive(false);
			}
			//Wait for a frame
			yield return null;
		}
	}
}
