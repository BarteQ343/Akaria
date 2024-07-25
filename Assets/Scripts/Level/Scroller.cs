using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroller : MonoBehaviour
{
	public float scrollSpeed = 2f;

	void Update()
    {
		Vector3 newPosition = transform.position;
		newPosition.x += scrollSpeed * Time.deltaTime;
		transform.position = newPosition;
	}
}
