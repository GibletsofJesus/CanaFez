using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveChanger : MonoBehaviour
{
	public static PerspectiveChanger instance;

	[SerializeField]
	Transform CameraObject, PlayerAvatar, minimapCam, WorldObjects;

	public Vector3 offset;
	public float lerpSpeed, changeSpeed;

	[SerializeField]
	float rotationAmount, idealRotation = 0;

	public float GetWorldOrientation ()
	{
		return Mathf.Abs(WorldObjects.transform.rotation.eulerAngles.y);
	}

	Vector3 prevVel, prevAngVel;

	public void RotatePerspective ()
	{
		if (rotationAmount > 0) {			
			rotationAmount -= Time.unscaledDeltaTime * changeSpeed;
			WorldObjects.RotateAround(PlayerAvatar.position,Vector3.up,Time.unscaledDeltaTime * changeSpeed);

			if (rotationAmount < 0) {

				WorldObjects.RotateAround(PlayerAvatar.position,Vector3.up,rotationAmount);

				rotationAmount = 0;

				Time.timeScale = Input.GetKey(KeyCode.Joystick1Button4) ? 0 : 1;
				/*if (!Input.GetKey(KeyCode.Joystick1Button4)) {
					m_Rigidbody.velocity = prevVel;
					m_Rigidbody.angularVelocity = prevAngVel;
					m_Rigidbody.isKinematic = false;
				}*/
			}
		}
		else if (rotationAmount < 0) {
				rotationAmount += Time.unscaledDeltaTime * changeSpeed;
				WorldObjects.RotateAround(PlayerAvatar.position,Vector3.down,Time.unscaledDeltaTime * changeSpeed);

				if (rotationAmount > 0) {

					WorldObjects.RotateAround(PlayerAvatar.position,Vector3.up,rotationAmount);

					//WorldObjects.rotation = Quaternion.Euler(0,idealRotation,0);
					rotationAmount = 0;

					Time.timeScale = Input.GetKey(KeyCode.Joystick1Button4) ? 0 : 1;
					/*
					if (!Input.GetKey(KeyCode.Joystick1Button4)) {
						m_Rigidbody.velocity = prevVel;
						m_Rigidbody.angularVelocity = prevAngVel;
						m_Rigidbody.isKinematic = false;
					}*/
				}
			}
	}

	bool forceRotation;
	Quaternion accurateRot, startRot;
	Rigidbody m_Rigidbody;

	void Start ()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
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
		lerpSpeed = 6;
		GetComponent<Rigidbody>().isKinematic = false;
	}

	//Round camera rotation to nearest 15 degrees
	Quaternion snapRot ()
	{
		Vector3 ea = accurateRot.eulerAngles;
		return Quaternion.Euler(new Vector3 (Mathf.Round(ea.x / 15) * 15, Mathf.Round(ea.y / 5) * 5, Mathf.Round(ea.z / 15) * 15));
	}

	float ElLerpo = 0;
	public Vector3 idealPosition;

	public int minimapSnap;

	// Update is called once per frame
	void Update ()
	{
		minimapCam.position = PlayerAvatar.transform.position + (Vector3.up * 137);
		minimapCam.position = new Vector3 (
			((int)minimapCam.transform.position.x / minimapSnap) * minimapSnap,
			((int)minimapCam.transform.position.y / minimapSnap) * minimapSnap,
			((int)minimapCam.transform.position.z / minimapSnap) * minimapSnap
		);
		if (forceRotation) {
			ElLerpo += Time.deltaTime;
			accurateRot = Quaternion.Lerp(startRot,Quaternion.Euler(Vector3.down * 180),ElLerpo);
			CameraObject.localRotation = snapRot();
		}

		if (!ThirdPersonCharacter.instance.rolling)
			offset.x = 9.25f * (transform.rotation.eulerAngles.y > 0 && transform.rotation.eulerAngles.y < 180 ? 1 : -1);
		

		Vector3 clampedPos = PlayerAvatar.position;
		if (clampedPos.y < -15)
			clampedPos.y = -15;
		idealPosition = Vector3.Lerp(CameraObject.position,clampedPos + offset,Time.deltaTime * lerpSpeed);
		idealPosition.y = -5;
		CameraObject.position = idealPosition;
		RotatePerspective();
		ControllerInput();

	}

	public void Rotate (bool dir)//false left, true right
	{
		rotationAmount = 90 * (dir ? 1 : -1);
		idealRotation += rotationAmount;
		if (idealRotation < 0)
			idealRotation += 360;
		if (idealRotation > 360)
			idealRotation -= 360;

		//Equivelant of pausing this ting
		prevVel = m_Rigidbody.velocity;
		prevAngVel = m_Rigidbody.angularVelocity;
		//m_Rigidbody.isKinematic = true;
		Time.timeScale = 0;
	}

	public void Rotate (float amount)
	{
		if (amount == 270)
			amount = -90;
		if (amount == -270)
			amount = 90;
		rotationAmount += amount;
		idealRotation += rotationAmount;
		if (idealRotation < 0)
			idealRotation += 360;
		if (idealRotation > 360)
			idealRotation -= 360;
	}

	void ControllerInput ()
	{
		if (Input.GetKey(KeyCode.Joystick1Button5) && rotationAmount == 0) {
			Rotate(true);
		}
		if (Input.GetKey(KeyCode.Joystick1Button4) && rotationAmount == 0) {
			Rotate(false);
		}
	}
}
