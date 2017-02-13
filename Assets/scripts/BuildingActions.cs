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

	public void Trigger ()
	{
		triggered = true;
	}

	void OnTriggerEnter (Collider col)
	{
		if (!triggered) {
			foreach (BuildingActions b in associatedTriggers) {
				b.Trigger();
			}
			minimapHighlight.SetActive(true);
			triggered = true;
		}
		if (associatedBuilding) {
			if (associatedBuilding.spawner.gameObject.activeSelf)
				ThirdPersonCharacter.instance.lastSpawner = associatedBuilding.spawner;
		}
	}
}
