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
	AudioClip flashSound;
	[SerializeField]
	flash Antenna;
	[SerializeField]
	int pointsForLandingHere = 100;

	public void Trigger ()
	{
		triggered = true;
		if (roofObjects.Length > 0)
			StartCoroutine(flashRoof());
	}

	IEnumerator flashRoof ()
	{
		while (!PlayerCharacter.instance.isGrounded())
			yield return null;

		if (flashSound) {
			SoundManager.instance.playSound(flashSound,1,Random.Range(.85f,1.15f));
			UIScoreManager.instance.SpawnText(Camera.main.WorldToViewportPoint(PlayerCharacter.instance.transform.position),pointsForLandingHere);
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

	void OnTriggerEnter (Collider col)
	{
		if (associatedBuilding) {
			if (associatedBuilding.spawner.gameObject.activeSelf) {
				if (PlayerCharacter.instance.lastSpawner != associatedBuilding.spawner || GameTimer.instance.timeElapsed < 3)
					StartCoroutine(SetSpawner(triggered));
			}
		}
		if (!triggered) {
			foreach (BuildingActions b in associatedTriggers) {
				b.Trigger();
			}
			if (roofObjects.Length > 0) {
				minimapCapture.instance.Capture(pointsForLandingHere);
				StartCoroutine(flashRoof());
			}
			minimapHighlight.SetActive(true);
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

	IEnumerator SetSpawner (bool b)
	{
		if (activeSpawner)
			activeSpawner.DeselectSpawner();
		Antenna.enabled = true;

		if (b)
			SoundManager.instance.playSound(flashSound,1,Random.Range(.85f,1.15f));			
		
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
