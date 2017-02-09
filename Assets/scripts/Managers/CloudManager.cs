using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
	public static CloudManager instance;

	[SerializeField]
	GameObject cloudPrefab, worldObjects;

	public int maxClouds = 30;
	//Interval between clouds being spawned
	public float SpawnRate, CloudLifeTime;
	float spawnCD;
	[HideInInspector]
	public List<GameObject> DeadClouds = new List<GameObject> ();
	public List<GameObject> ActiveClouds = new List<GameObject> ();

	// Use this for initialization
	void Start ()
	{
		instance = this;	
	}

	//some kind of pre-warmer would be rad af

	void SpawnCloud (Vector3 spawnPos, Vector3 spawnRot, Vector3 direction, float scale)
	{
		GameObject newCloud;
		if (DeadClouds.Count > 0) {
			Debug.Log("ressurection");
			newCloud = DeadClouds [0];
			DeadClouds.Remove(newCloud);
			newCloud.SetActive(true);
			newCloud.transform.position = spawnPos;
			newCloud.transform.rotation = Quaternion.Euler(spawnRot);
			newCloud.transform.parent = worldObjects.transform;
		}
		else {
			newCloud = GameObject.Instantiate(cloudPrefab,spawnPos,Quaternion.Euler(spawnRot),worldObjects.transform);
		}
		newCloud.transform.localScale = Vector3.one * scale;
		ActiveClouds.Add(newCloud);
		Cloud cloudData = newCloud.GetComponent<Cloud>();
		cloudData.SetupCloud(direction,CloudLifeTime);
	}

	// Update is called once per frame
	void Update ()
	{
		spawnCD -= Time.deltaTime;

		if (spawnCD <= 0 && ActiveClouds.Count < maxClouds) {
			Vector3 spawnRot, direction;
			spawnRot = new Vector3 (Random.Range(-30,30), Random.Range(180,-180), Random.Range(-15,15));
			direction = new Vector3 (Random.Range(2,5), 0, Random.Range(2,5));
			SpawnCloud(transform.position,spawnRot,direction,Random.Range(0.85f,1.15f));
			spawnCD = SpawnRate;
		}
	}
}
