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
	int displayPoints, points;

	public void Setup (Vector3 startPos, int _points)
	{
		flashing = false;
		displayPoints = 0;
		points = _points;
		s = "+" + displayPoints;
		foreach (TextMesh t in TextElements) {
			t.text = s;
		}
		//transform.localPosition = startPos;
	}

	[SerializeField]
	bool flashing;

	IEnumerator endFlash ()
	{
		flashing = true;

		if (displayPoints < points)
			yield return null;
		
		for (int i = 0; i < 10; i++) {
			foreach (TextMesh t in TextElements) {
				t.text = "";
			}
			yield return new WaitForEndOfFrame ();
			yield return new WaitForEndOfFrame ();
			yield return new WaitForEndOfFrame ();
			yield return new WaitForEndOfFrame ();
			foreach (TextMesh t in TextElements) {
				t.text = s;
			}
			yield return new WaitForEndOfFrame ();
			yield return new WaitForEndOfFrame ();
			yield return new WaitForEndOfFrame ();
			yield return new WaitForEndOfFrame ();
		}
		ReturnToPool();
	}

	void ReturnToPool ()
	{
		UIScoreManager.instance.ActiveTexts.Remove(gameObject);
		UIScoreManager.instance.InactiveTexts.Add(gameObject);
		gameObject.SetActive(false);
	}

	void Update ()
	{
		if (displayPoints < points) {
			if (points - displayPoints > 1) {
				displayPoints += 2;
				UIScoreManager.instance.points += 2;
			}
			else {
				displayPoints++;
				UIScoreManager.instance.points++;
			}
			s = "+" + displayPoints;
			foreach (TextMesh t in TextElements) {
				t.text = s;
			}
		}
		transform.localRotation = Quaternion.Euler(Vector3.zero);

		//Round y pos to mulitples of 1.2
		if (transform.localPosition.y < 5 && !flashing) {
			transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y + Time.deltaTime * moveSpeed, 0);
		}
		else if (!flashing) {
				StartCoroutine(endFlash());
			}
	}
}
