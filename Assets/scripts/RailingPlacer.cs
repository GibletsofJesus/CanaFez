using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailingPlacer : MonoBehaviour
{

	public int sizeX = 0, sizeY = 0;
	[SerializeField]
	float chunkSize = 0.5375f;
	[SerializeField]
	GameObject railingObject;
	// Use this for initialization
	void Start ()
	{
		MakeRailings(sizeX,sizeY);
	}

	List<GameObject> rails = new List<GameObject> ();

	public void MakeRailings (int _sizeX, int _sizeY)
	{
		//For square sections of railings, add 2 to sizeY
		if (rails.Count > 0) {
			foreach (GameObject g in rails) {
				Destroy(g);
			}
			rails.Clear();
		}
		for (int x = 0; x < _sizeX; x++) {
			for (int y = 0; y < _sizeY; y++) {
				if (y == 0 || y == _sizeY - 1) {
					GameObject newRail = Instantiate(railingObject,transform) as GameObject;
					Vector3 spawnPos = new Vector3 (chunkSize * (y == 0 ? y : y - 1), 0, -chunkSize * (y == 0 ? x : x + 1));
					newRail.transform.localPosition = spawnPos;
					Quaternion spawnRot = Quaternion.Euler(y == 0 ? Vector3.zero : Vector3.up * 180);
					newRail.transform.localRotation = spawnRot;
					rails.Add(newRail);
					//newRail.transform.localPosition += new Vector3 (-(float)_sizeY / 2, 0, (float)_sizeX / 2);
				}
				else if (x == 0 || x == _sizeX - 1) {

						GameObject newRail = Instantiate(railingObject,transform) as GameObject;
						Vector3 spawnPos = new Vector3 (chunkSize * (x == 0 ? y : y - 1), 0, -chunkSize * (x == 0 ? x : x + 1));
						newRail.transform.localPosition = spawnPos;
						Quaternion spawnRot = Quaternion.Euler(x == 0 ? Vector3.up * 90 : Vector3.up * 270);
						newRail.transform.localRotation = spawnRot;
						rails.Add(newRail);
						//newRail.transform.localPosition += new Vector3 (-(float)_sizeY / 2, 0, (float)_sizeX / 2);
					}
			}
		}
		old = _sizeX * _sizeY;
	}

	int old;
	// Update is called once per frame
	void Update ()
	{
		if (sizeX * sizeY != old) {
			MakeRailings(sizeX,sizeY);
		}
	}
}
