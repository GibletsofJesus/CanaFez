using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingActions : MonoBehaviour
{
	[SerializeField]
	GameObject minimapHighlight;
	bool triggered;
	[SerializeField]
	BuildingActions[] associatedTriggers;
	[SerializeField]
	Building associatedBuilding;
	[SerializeField]
	Renderer roofObject;
	[SerializeField]
	Texture2D[] textures;
	[SerializeField]
	AudioClip flashSound;

	public void Trigger ()
	{
		triggered = true;
		if (roofObject)
			StartCoroutine(flashRoof());
	}

	IEnumerator flashRoof ()
	{
		while (!PlayerCharacter.instance.isGrounded())
			yield return null;

		if (flashSound) {
			SoundManager.instance.playSound(flashSound,1,Random.Range(.85f,1.15f));
		}

		for (int i = 0; i < 2; i++) {
			roofObject.material.mainTexture = textures [1];
			yield return new WaitForSeconds (0.05f);
			roofObject.material.mainTexture = textures [0];
			yield return new WaitForSeconds (0.05f);
		}
	}

	void OnTriggerEnter (Collider col)
	{
		if (!triggered) {
			foreach (BuildingActions b in associatedTriggers) {
				b.Trigger();
			}
			if (roofObject) {
				StartCoroutine(flashRoof());
			}
			minimapHighlight.SetActive(true);
			triggered = true;
		}
		if (associatedBuilding) {
			if (associatedBuilding.spawner.gameObject.activeSelf)
				PlayerCharacter.instance.lastSpawner = associatedBuilding.spawner;
		}
	}
}
