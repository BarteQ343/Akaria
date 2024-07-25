using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustController : MonoBehaviour
{
	public GameObject dustEffectPrefab; // Reference to the dust effect prefab
	[HideInInspector] 
	public Transform feetPosition;      // Position where the dust effect should be instantiated

	private Animator dustAnimator;

	private void Start()
	{
		if (feetPosition == null)
		{
			feetPosition = transform; // Default to the character's position
		}
	}

	public void CreateDust(string jumpLand)
	{
		GameObject dust = Instantiate(dustEffectPrefab, feetPosition.position, Quaternion.identity);
		dustAnimator = dust.GetComponent<Animator>();
		if (jumpLand == "Jump")
		{
			dustAnimator.Play("DustJump");
		}
		if (jumpLand == "Land")
		{
			dustAnimator.Play("DustLand");
		}
		Destroy(dust, dustAnimator.GetCurrentAnimatorStateInfo(0).length); // Destroy after animation
	}
}
