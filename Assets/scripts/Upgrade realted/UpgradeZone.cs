using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeZone : MonoBehaviour
{
	bool collected;
	[SerializeField]
	Animator zone;

	void Start ()
	{
		
	}

	void OnTriggerEnter (Collider col)
	{
		if (col.tag == "Player" && !collected) {
			if (UpgradeManager.instance.availableUpgrades.Count > 0)
				GameStateManager.instance.ChangeState(GameStateManager.GameStates.STATE_UPGRADE);
			zone.Play("pointer_stop");
			collected = true;
		}
	}
}
