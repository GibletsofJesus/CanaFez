using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
	public Transform target;
	public bool mimic;

	float DontBother = 1;

	// Update is called once per frame
	void Update ()
	{
		DontBother -= Time.deltaTime;
		if (DontBother < 0) {
			DontBother = .5f;
			if (!mimic)
				transform.LookAt(target);
			else
				transform.rotation = target.rotation;
		}
	}
}
