using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScoreManager : MonoBehaviour
{
	public static UIScoreManager instance;
	[SerializeField]
	GameObject prefab;

	[HideInInspector]
	public List<GameObject> InactiveTexts = new List<GameObject> ();
	public List<GameObject> ActiveTexts = new List<GameObject> ();

	void Update ()
	{
		if (!instance)
			instance = this;
	}

	public void SpawnText (Vector3 spawnPos)
	{
		GameObject newText;
		if (InactiveTexts.Count > 0) {
			newText = InactiveTexts [0];
			InactiveTexts.Remove(newText);
			newText.SetActive(true);
			newText.transform.position = spawnPos;
			//newText.transform.rotation = Quaternion.Euler(Vector3.left * 90);
		}
		else {
			newText = GameObject.Instantiate(prefab,spawnPos,Quaternion.Euler(Vector3.left * 90),transform);
		}
		ActiveTexts.Add(newText);
		scoreText textData = newText.GetComponent<scoreText>();
		textData.Setup(spawnPos,100);
	}
}
