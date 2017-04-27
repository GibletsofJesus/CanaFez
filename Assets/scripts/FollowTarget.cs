using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
	public Transform target;
	public Vector3 offset = new Vector3 (0f, 7.5f, 0f);
	public Vector3 defaultRotation = new Vector3 (0f, 0f, 0f);
	public int snap = 4;
	public bool mimicRot, specialRot;
	//SpecialRot is just for doing y axis of rot

	void Update ()
	{
		transform.position = target.position + offset;
		if (snap > 0) {
			transform.position = new Vector3 (
				((int)transform.position.x / snap) * snap,
				((int)transform.position.y / snap) * snap,
				((int)transform.position.z / snap) * snap
			);
		}
		if (mimicRot)
			transform.rotation = target.rotation;
		
		if (specialRot && !PlayerCharacter.instance.rolling)
			transform.rotation = Quaternion.Euler(defaultRotation.x,target.localRotation.eulerAngles.y,defaultRotation.z);
	}
}
