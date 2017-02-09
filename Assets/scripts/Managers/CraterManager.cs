using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraterManager : MonoBehaviour
{

	public static CraterManager instance;

	[SerializeField]
	GameObject craterPrefab, worldObjects;

	//Interval between clouds being spawned
	public float lifeTime;
	[HideInInspector]
	public List<GameObject> DeadCraters = new List<GameObject> ();
	public List<GameObject> ActiveCraters = new List<GameObject> ();

	// Use this for initialization
	void Start ()
	{
		instance = this;	
	}

	public void SpawnCrater (Vector3 spawnPos)
	{
		GameObject newCrater;
		if (DeadCraters.Count > 0) {
			newCrater = DeadCraters [0];
			DeadCraters.Remove(newCrater);
			newCrater.SetActive(true);
			newCrater.transform.position = spawnPos;
			newCrater.transform.rotation = Quaternion.Euler(Vector3.left * 90);
		}
		else {
			newCrater = GameObject.Instantiate(craterPrefab,spawnPos,Quaternion.Euler(Vector3.left * 90),worldObjects.transform);
		}
		ActiveCraters.Add(newCrater);
		Crater craterData = newCrater.GetComponent<Crater>();
		craterData.Setup(lifeTime);
	}
}
