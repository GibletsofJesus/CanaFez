using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveChanger : MonoBehaviour
{
	public static PerspectiveChanger instance;

	[SerializeField]
	Transform CameraObject, PlayerAvatar, minimapCam, WorldObjects;

	[SerializeField]
	Vector3 offset;
	public float lerpSpeed, changeSpeed;

	float rotationAmount, idealRotation = 0;

	bool rotating;

	public void RotatePerspective ()
	{
		if (rotationAmount > 0) {
			Time.timeScale = 0f;
			rotationAmount -= Time.unscaledDeltaTime * changeSpeed;
			WorldObjects.RotateAround(PlayerAvatar.position,Vector3.up,Time.unscaledDeltaTime * changeSpeed);

			if (rotationAmount < 0) {

				WorldObjects.RotateAround(PlayerAvatar.position,Vector3.up,rotationAmount);

				//WorldObjects.rotation = Quaternion.Euler(0,idealRotation,0);
				rotating = false;		
				rotationAmount = 0;
				Time.timeScale = 1;
			}
		}
		else if (rotationAmount < 0) {
				Time.timeScale = 0f;
				rotationAmount += Time.unscaledDeltaTime * changeSpeed;
				WorldObjects.RotateAround(PlayerAvatar.position,Vector3.down,Time.unscaledDeltaTime * changeSpeed);

				if (rotationAmount > 0) {

					WorldObjects.RotateAround(PlayerAvatar.position,Vector3.up,rotationAmount);

					//WorldObjects.rotation = Quaternion.Euler(0,idealRotation,0);
					rotating = false;
					rotationAmount = 0;
					Time.timeScale = 1;
				}
			}
			else {
				//Time.timeScale = 1;
			}
	}

	bool forceRotation;
	Quaternion accurateRot, startRot;

	void Start ()
	{
		instance = this;
		accurateRot = CameraObject.localRotation;
		startRot = accurateRot;
		StartCoroutine(MoveCam());
	}

	public IEnumerator MoveCam ()
	{
		yield return new WaitForSeconds (3);
		lerpSpeed = 2;
		yield return new WaitForSeconds (1);
		forceRotation = true;
		yield return new WaitForSeconds (1);
		GameStateManager.instance.ChangeState(GameStateManager.GameStates.STATE_GAMEPLAY);
		GetComponent<Rigidbody>().isKinematic = false;
	}

	Quaternion snapRot ()
	{
		Vector3 ea = accurateRot.eulerAngles;
		return Quaternion.Euler(new Vector3 (Mathf.Round(ea.x / 15) * 15, Mathf.Round(ea.y / 5) * 5, Mathf.Round(ea.z / 15) * 15));
	}

	float ElLerpo = 0;
	public Vector3 idealPosition;
	// Update is called once per frame
	void Update ()
	{
		minimapCam.position = PlayerAvatar.transform.position + (Vector3.up * 137);
		if (forceRotation) {
			ElLerpo += Time.deltaTime;
			accurateRot = Quaternion.Lerp(startRot,Quaternion.Euler(Vector3.down * 180),ElLerpo);
			CameraObject.localRotation = snapRot();
		}
		if (transform.rotation.eulerAngles.y > 0 && transform.rotation.eulerAngles.y < 180)
			offset.x *= -1f;
		
		idealPosition = Vector3.Lerp(CameraObject.position,PlayerAvatar.position + offset,Time.deltaTime * lerpSpeed);
		CameraObject.position = idealPosition;
		//CameraObject.position = new Vector3 (CameraObject.position.x, 0.25f, CameraObject.position.z);
		RotatePerspective();
		//KeyboardInput();
		ControllerInput();

		if (transform.rotation.eulerAngles.y > 0 && transform.rotation.eulerAngles.y < 180)
			offset.x *= -1f;
	}

	void KeyboardInput ()
	{
		if (Input.GetKeyDown(KeyCode.LeftArrow) && !rotating) {

			//StartCoroutine(RotatePerspective(true));
			rotating = true;
			rotationAmount = 90;
			idealRotation += rotationAmount;
			if (idealRotation < 0)
				idealRotation += 360;
		}

		if (Input.GetKeyDown(KeyCode.RightArrow) && !rotating) {
			//StartCoroutine(RotatePerspective(false));
			rotating = true;
			rotationAmount = -90;
			idealRotation += rotationAmount;
			if (idealRotation > 360)
				idealRotation -= 360;
		}
	}

	void ControllerInput ()
	{
		if (Input.GetKey(KeyCode.Joystick1Button5) && !rotating) {
			rotating = true;
			rotationAmount = 90;
			idealRotation += rotationAmount;
			if (idealRotation < 0)
				idealRotation += 360;
		}
		if (Input.GetKey(KeyCode.Joystick1Button4) && !rotating) {
			rotating = true;
			rotationAmount = -90;
			idealRotation += rotationAmount;
			if (idealRotation > 360)
				idealRotation -= 360;
		}
	}
}
