using UnityEngine;
using System.Collections;

public class rotater : MonoBehaviour
{

	public Transform rotationCenter;
	public axis rotationAxis;

	public enum axis
	{
		x,
		y,
		z,
	}

	public float speed;
	float angle;

	void FixedUpdate ()
	{
		switch (rotationAxis) {
			case axis.x:
				transform.RotateAround(rotationCenter.position,Vector3.right,Time.fixedDeltaTime * speed);
				break;
			case axis.y:
				transform.RotateAround(rotationCenter.position,Vector3.up,Time.fixedDeltaTime * speed);
				break;
			case axis.z:
				transform.RotateAround(rotationCenter.position,Vector3.forward,Time.fixedDeltaTime * speed);
				break;
		}
	}
}