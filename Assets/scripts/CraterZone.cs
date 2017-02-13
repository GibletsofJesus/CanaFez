using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraterZone : MonoBehaviour
{
	
	void OnTriggerEnter (Collider col)
	{
		ThirdPersonCharacter.instance.SetCraters(true);
	}

	void OnTriggerStay (Collider col)
	{
		ThirdPersonCharacter.instance.SetCraters(true);
	}

	void OnTriggerExit (Collider col)
	{
		ThirdPersonCharacter.instance.SetCraters(false);
	}
}
