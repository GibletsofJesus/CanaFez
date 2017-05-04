using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingActions : MonoBehaviour
{
	public static BuildingActions activeSpawner;

	[SerializeField]
	GameObject minimapHighlight;
	bool triggered;
	[SerializeField]
	BuildingActions[] associatedTriggers;
	[SerializeField]
	Building associatedBuilding;
	[SerializeField]
	Renderer[] roofObjects;
	[SerializeField]
	Texture2D[] textures;
	[SerializeField]
	AudioClip flashSound, spawnerSound;
	[SerializeField]
	flash Antenna;
	[SerializeField]
	int pointsForLandingHere = 100;

	public void Trigger ()
	{
		StartCoroutine(landingEffect());
		triggered = true;
	}

	IEnumerator landingEffect ()
	{
		if (roofObjects.Length > 0) {
			while (!PlayerCharacter.instance.isGrounded())
				yield return null;

			if (!PlayerCharacter.instance.respawning) {
				if (associatedBuilding) {
					if (associatedBuilding.spawner.gameObject.activeSelf) {
						if (PlayerCharacter.instance.lastSpawner != associatedBuilding.spawner || GameTimer.instance.timeElapsed < 3) {
							Antenna.GetComponentInChildren<Animator>().enabled = true;
							Antenna.GetComponentInChildren<Animator>().Play("pointer_ZOOP");
							StartCoroutine(SetSpawner());
						}
					}
					else
						spawnerSound = null;
				}

				if (GameTimer.instance.timeElapsed > 3)
					minimapCapture.instance.Capture(PlayerCharacter.instance.GetPointsForDistance());
				minimapHighlight.SetActive(true);

				if (flashSound) {
					if (GameTimer.instance.timeElapsed > 3) {
						UIScoreManager.instance.SpawnText(Camera.main.WorldToViewportPoint(PlayerCharacter.instance.transform.position),PlayerCharacter.instance.GetPointsForDistance());
						//make pitch dendant on score
						SoundManager.instance.playSound(flashSound,spawnerSound ? 0 : 1,Mathf.Lerp(0.75f,3f,(float)(PlayerCharacter.instance.GetPointsForDistance() - 100) / 420f));
					}
					else {
						SoundManager.instance.playSound(flashSound,spawnerSound ? 0 : 1,Random.Range(.85f,1.15f));
					}
				}

				for (int i = 0; i < 2; i++) {
					foreach (Renderer r in roofObjects)
						r.material.mainTexture = textures [1];
					yield return new WaitForSeconds (0.05f);
					foreach (Renderer r in roofObjects)
						r.material.mainTexture = textures [0];
					yield return new WaitForSeconds (0.05f);
				}
			}
		}
	}

	void OnTriggerEnter (Collider col)
	{
		if (!triggered) {
			foreach (BuildingActions b in associatedTriggers) {
				b.Trigger();
			}
			StartCoroutine(landingEffect());
			triggered = true;
		}
	}

	public void DeselectSpawner ()
	{
		foreach (LineRenderer lr in Antenna.lrs) {
			lr.startColor = Antenna.meshCols [1];
			lr.endColor = Antenna.meshCols [1];
		}
	}

	IEnumerator SetSpawner ()
	{
		if (activeSpawner)
			activeSpawner.DeselectSpawner();
		Antenna.enabled = true;

		UIScoreManager.instance.SpawnText(Camera.main.WorldToViewportPoint(Antenna.transform.position),scoreText.textType.checkpoint);

		SoundManager.instance.playSound(spawnerSound,1,Random.Range(.85f,1.15f));			
		
		yield return	new WaitForSeconds (0.75f);
		Antenna.enabled = false;
		foreach (LineRenderer lr in Antenna.lrs) {
			lr.startColor = Antenna.meshCols [0];
			lr.endColor = Antenna.meshCols [0];
		}
		activeSpawner = this;
		PlayerCharacter.instance.lastSpawner = associatedBuilding.spawner;
	}
}
