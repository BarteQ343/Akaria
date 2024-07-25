using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayInBackground : MonoBehaviour
{
	public static PlayInBackground Instance { get; private set; }
	void Awake()
    {
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}
}
