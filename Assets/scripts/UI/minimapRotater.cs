using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minimapRotater : MonoBehaviour
{

	float rotationAmount;
	[SerializeField]
	Transform rotationCenter;
	public bool dir;
	// Update is called once per frame
	void Update ()
	{
		if (rotationAmount > 0) {
			transform.RotateAround(rotationCenter.position,Vector3.up,Time.unscaledDeltaTime * PerspectiveChanger.instance.changeSpeed);
			rotationAmount -= Time.unscaledDeltaTime * PerspectiveChanger.instance.changeSpeed;

			if (rotationAmount < 0) {
				Snap();
			}
		}
		else if (rotationAmount < 0) {			
				rotationAmount += Time.unscaledDeltaTime * PerspectiveChanger.instance.changeSpeed;
				transform.RotateAround(rotationCenter.position,Vector3.down,Time.unscaledDeltaTime * PerspectiveChanger.instance.changeSpeed);

				if (rotationAmount > 0) {
					Snap();
				}
			}
	}

	public void Snap ()
	{
		switch ((int)PerspectiveChanger.instance.RoundToNearest90Degrees(transform.localRotation.eulerAngles.z)) {
			case 0:
				transform.localRotation = Quaternion.Euler(0,0,0);
				transform.localPosition = new Vector3 (dir ? -20 : 20, 0, 0);
				break;
			case 90:
				transform.localRotation = Quaternion.Euler(0,0,90);
				transform.localPosition = new Vector3 (0, dir ? -20 : 20, 0);
				break;
			case 180:
				transform.localRotation = Quaternion.Euler(0,0,180);
				transform.localPosition = new Vector3 (dir ? 20 : -20, 0, 0);
				break;
			case 270:
				transform.localRotation = Quaternion.Euler(0,0,270);
				transform.localPosition = new Vector3 (0, dir ? 20 : -20, 0);
				break;

		}
		rotationAmount = 0;
	}

	public void SetRotation (float f)
	{
		rotationAmount = f;
	}
}
