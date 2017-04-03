using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
	public Vector3 rotOffset, standardPos;
	[SerializeField]
	Transform pavement;
	[SerializeField]
	public Animator spawner, upgradeZone;
	public float result;
	[SerializeField]
	AudioSource radio;
	[SerializeField]
	AudioClip[] tunes;

	public void SetPavementHeight (float f)
	{
		pavement.position = new Vector3 (pavement.position.x, f, pavement.position.z);
	}

	public Animator SetupSpawner (bool respawn)
	{
		if (radio) {
			radio.clip = tunes [Random.Range(0,tunes.Length)]; 
			radio.Play();
			radio.loop = true;
		}

		if (upgradeZone)
			UpgradeManager.instance.AllZones.Add(upgradeZone);

		if (spawner) {
			spawner.gameObject.SetActive(respawn); 
			if (respawn) {
				switch (Random.Range(0,3)) {
					case 0:
						spawner.transform.localRotation = Quaternion.Euler(Vector3.zero);
						spawner.transform.localPosition = new Vector3 (0.25f, 0.5f, Random.Range(0.25f,-0.25f));
						break;
					case 1:
						spawner.transform.localRotation = Quaternion.Euler(Vector3.up * 90);
						spawner.transform.localPosition = new Vector3 (Random.Range(0.25f,-0.25f), 0.5f, -0.25f);
						break;
					case 2:
						spawner.transform.localRotation = Quaternion.Euler(Vector3.up * 180);
						spawner.transform.localPosition = new Vector3 (-0.25f, 0.5f, Random.Range(0.25f,-0.25f));
						break;
					case 3:
						spawner.transform.localRotation = Quaternion.Euler(Vector3.up * 270);
						spawner.transform.localPosition = new Vector3 (Random.Range(0.25f,-0.25f), 0.5f, 0.25f);
						break;
				}
			}
			return spawner.gameObject.activeSelf ? spawner : null;
		}
		return null;
	}
}
