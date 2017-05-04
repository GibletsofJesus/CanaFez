using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScoreManager : MonoBehaviour
{
	public static UIScoreManager instance;
	[SerializeField]
	GameObject prefab, attachScoresToMe, attachEndGameUIScoresToMe;
	[SerializeField]
	Text UIprefab;
	public int points;
	[SerializeField]
	TextMesh[] scoreDisplay;

	[HideInInspector]
	public List<GameObject> InactiveTexts = new List<GameObject> ();
	[HideInInspector]
	public List<GameObject> ActiveTexts = new List<GameObject> ();

	//[HideInInspector]
	public List<Text> InactiveUITexts = new List<Text> ();
	public List<Text> ActiveUITexts = new List<Text> ();

	void Update ()
	{
		if (!instance)
			instance = this;

		foreach (TextMesh tm in scoreDisplay) {
			tm.text = "SCORE: " + points;
		}

		foreach (Text t in ActiveUITexts) {
			//move em up
			//fade em out
			//then turn em off
			//go up to 511

			t.rectTransform.anchoredPosition += Vector2.up * Time.deltaTime * speed;
			t.color = new Color (.9f, .9f, .9f, Mathf.Lerp(1,0,(t.rectTransform.anchoredPosition.y - 136f) / 375f));
			if (t.rectTransform.anchoredPosition.y > 511) {
				ActiveUITexts.Remove(t);
				InactiveUITexts.Add(t);
				t.gameObject.SetActive(false);
				break;
			}
		}
	}

	public float speed = 5;

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
			newText = GameObject.Instantiate(prefab,attachScoresToMe.transform);
		}
		newText.transform.localScale = Vector3.one / 66;
		newText.transform.localPosition = new Vector3 (Mathf.Lerp(-16.25f,16.25f,spawnPos.x), Mathf.Lerp(-11.4f,11.4f,spawnPos.y), 0);
		ActiveTexts.Add(newText);
		scoreText textData = newText.GetComponent<scoreText>();
		textData.Setup(points);
	}

	public void SpawnText (Vector3 spawnPos, scoreText.textType _type)
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
			newText = GameObject.Instantiate(prefab,attachScoresToMe.transform);
		}
		newText.transform.localScale = Vector3.one / 66;
		newText.transform.localPosition = (_type == scoreText.textType.death ? 
			new Vector3 (-12f, 8.5f, 0) : 
			new Vector3 (Mathf.Lerp(-16.25f,16.25f,spawnPos.x), Mathf.Lerp(-11.4f,11.4f,spawnPos.y), 0));
		ActiveTexts.Add(newText);
		scoreText textData = newText.GetComponent<scoreText>();
		textData.Setup(_type);
	}

	public void SpawnEndGameText (int points)
	{
		Text newText;
		if (InactiveUITexts.Count > 0) {
			newText = InactiveUITexts [0];
			InactiveUITexts.Remove(newText);
			newText.gameObject.SetActive(true);
			//newText.transform.rotation = Quaternion.Euler(Vector3.left * 90);
		}
		else {
			newText = (Text)GameObject.Instantiate(UIprefab,attachEndGameUIScoresToMe.transform);
		}
		newText.text = "+" + points;
		newText.rectTransform.anchoredPosition = new Vector2 (1700, 136);
		ActiveUITexts.Add(newText);
	}
}
