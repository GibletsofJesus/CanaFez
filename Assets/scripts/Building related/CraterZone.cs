using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraterZone : MonoBehaviour
{
	
	void OnTriggerEnter (Collider col)
	{
		PlayerCharacter.instance.SetCraters(true);
	}

	void OnTriggerStay (Collider col)
	{
		PlayerCharacter.instance.SetCraters(true);
	}

	void OnTriggerExit (Collider col)
	{
		PlayerCharacter.instance.SetCraters(false);
	}
}
