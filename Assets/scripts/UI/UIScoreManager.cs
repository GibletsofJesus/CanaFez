using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScoreManager : MonoBehaviour
{
	public static UIScoreManager instance;
	[SerializeField]
	GameObject prefab;
	public int points;
	[SerializeField]
	TextMesh[] scoreDisplay;

	//[HideInInspector]
	public List<GameObject> InactiveTexts = new List<GameObject> ();
	public List<GameObject> ActiveTexts = new List<GameObject> ();

	void Update ()
	{
		if (!instance)
			instance = this;

		foreach (TextMesh tm in scoreDisplay) {
			tm.text = "SCORE: " + points;
		}
	}

	public void SpawnText (Vector3 spawnPos, int points)
	{
		GameObject newText;
		if (InactiveTexts.Count > 0) {
			newText = InactiveTexts [0];
			InactiveTexts.Remove(newText);
			newText.SetActive(true);
			newText.transform.localPosition = spawnPos;
			//newText.transform.rotation = Quaternion.Euler(Vector3.left * 90);
		}
		else {
			newText = GameObject.Instantiate(prefab,transform);
		}
		newText.transform.localScale = Vector3.one / 66;
		newText.transform.localPosition = new Vector3 (Mathf.Lerp(-16.25f,16.25f,spawnPos.x), Mathf.Lerp(-11.4f,11.4f,spawnPos.y), 0);
		ActiveTexts.Add(newText);
		scoreText textData = newText.GetComponent<scoreText>();
		textData.Setup(spawnPos,points);
	}
}
