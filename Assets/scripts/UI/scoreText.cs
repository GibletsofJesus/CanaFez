using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scoreText : MonoBehaviour
{

	[SerializeField]
	TextMesh[] TextElements;
	[SerializeField]
	float moveSpeed = 1;
	string s;

	public void Setup (Vector3 startPos, int points)
	{
		s = "+" + points;
		foreach (TextMesh t in TextElements) {
			t.text = s;
		}
		accuratePosition = startPos;
	}

	Vector3 accuratePosition;

	IEnumerator endFlash ()
	{
		foreach (TextMesh t in TextElements) {
			t.text = "";
		}
		yield return new WaitForEndOfFrame ();
		foreach (TextMesh t in TextElements) {
			t.text = s;
		}
	}

	void ReturnToPool ()
	{
		UIScoreManager.instance.ActiveTexts.Remove(gameObject);
		gameObject.SetActive(false);
	}

	void Update ()
	{
		//Round y pos to mulitples of 1.2
		if (transform.localPosition.y < 5) {
			accuratePosition.y += Time.deltaTime * moveSpeed;
			transform.localPosition = new Vector3 (accuratePosition.x, accuratePosition.y, 0);
		}
		else {
		
		}
		//Travel upwards until y pos euqals 5, then flash and dissapear
	}
}
