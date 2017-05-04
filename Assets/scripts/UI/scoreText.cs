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

	textType m_type;

	public enum textType
	{
		points,
		checkpoint,
		death
	}

	public void Setup (int _points)
	{
		m_type = textType.points;
		flashing = false;
		displayPoints = 0;
		points = _points;
		s = "+" + displayPoints;
		foreach (TextMesh t in TextElements) {
			t.text = s;
		}
		//transform.localPosition = startPos;
	}

	public void Setup (textType type)
	{
		m_type = type;
		flashing = false;
		displayPoints = 0;
		points = -1;
		s = type == textType.checkpoint ? "CHECKPOINT!" : "-15s";
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
		if (m_type == textType.points) {
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
		}

		if (m_type != textType.death) {
			if (transform.localPosition.y < (m_type == textType.points ? 5f : 6.5f) && !flashing) {
				transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y + Time.deltaTime * moveSpeed, 0);
			}
			else if (!flashing) {
					StartCoroutine(endFlash());
				}
		}
		else {
			if (transform.localPosition.y > 7 && !flashing) {
				transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y - Time.deltaTime * moveSpeed * .25f, 0);
			}
			else if (!flashing) {
					StartCoroutine(endFlash());
				}
		}
		transform.localRotation = Quaternion.Euler(Vector3.zero);
	}
}
