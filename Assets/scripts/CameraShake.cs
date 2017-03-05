using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraShake : MonoBehaviour
{
	public static CameraShake instance;

	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	public Transform camTransform;
	// How long the object should shake for.
	public float shakeDuration = 0f;
	
	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.7f;
	public float decreaseFactor = 1.0f;

	void OnEnable ()
	{
		instance = this;
	}

	public void Shake ()
	{
		shakeDuration += .5f;
	}

	void LateUpdate ()
	{
		if (GameStateManager.instance.GetState() == GameStateManager.GameStates.STATE_GAMEPLAY) {
			if (shakeDuration > 0) {
				camTransform.position = PerspectiveChanger.instance.idealPosition + Random.insideUnitSphere * shakeAmount;
				camTransform.position = new Vector3 (camTransform.position.x, camTransform.position.y, PerspectiveChanger.instance.idealPosition.z);

				shakeDuration -= Time.deltaTime * decreaseFactor;
			}
			else {
				shakeDuration = 0f;
				shakeAmount = 0;
				//camTransform.position = PerspectiveChanger.instance.idealPosition;
			}
		}
	}
}
