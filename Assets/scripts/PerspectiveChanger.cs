using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveChanger : MonoBehaviour
{
	public static PerspectiveChanger instance;

	public bool clockwise;

	[SerializeField]
	Transform CameraObject, PlayerAvatar, minimapCam, WorldObjects, miniMapQuad;

	public Vector3 offset;
	Vector3 adjustedOffset;
	public float lerpSpeed, changeSpeed;

	[SerializeField]
	float rotationAmount, idealRotation = 0;

	public float GetWorldOrientation ()
	{
		return CameraObject.transform.rotation.eulerAngles.y;
	}

	Vector3 prevVel, prevAngVel;

	public void RotatePerspective ()
	{
		if (rotationAmount > 0) {			
			rotationAmount -= Time.unscaledDeltaTime * changeSpeed;
			CameraObject.RotateAround(PlayerAvatar.position,Vector3.up,Time.unscaledDeltaTime * changeSpeed);

			//Need to also rotate the player
			Vector3 e = transform.localRotation.eulerAngles;
			e.y += Time.unscaledDeltaTime * changeSpeed;
			transform.localRotation = Quaternion.Euler(e);

			if (rotationAmount < 0) {
				CameraObject.localRotation = Quaternion.Euler(new Vector3 (0, RoundToNearest90Degrees(CameraObject.localRotation.eulerAngles.y), 0));
				PlayerCharacter.instance.UpdateMovementRestirctions(CameraObject.transform.localRotation.eulerAngles.y);
				rotationAmount = 0;

				checkFinishRotating(true);
			}
		}
		else if (rotationAmount < 0) {
				rotationAmount += Time.unscaledDeltaTime * changeSpeed;
				CameraObject.RotateAround(PlayerAvatar.position,Vector3.down,Time.unscaledDeltaTime * changeSpeed);

				Vector3 e = transform.localRotation.eulerAngles;
				e.y -= Time.unscaledDeltaTime * changeSpeed;
				transform.localRotation = Quaternion.Euler(e);

				if (rotationAmount > 0) {
					CameraObject.localRotation = Quaternion.Euler(new Vector3 (0, RoundToNearest90Degrees(CameraObject.localRotation.eulerAngles.y), 0));
					PlayerCharacter.instance.UpdateMovementRestirctions(CameraObject.transform.localRotation.eulerAngles.y);
					rotationAmount = 0;

					checkFinishRotating(false);
				}
			}
	}

	void checkFinishRotating (bool dir)
	{
		//If we're not finished, register input and make apporopriate andjustments to velocity.
		Time.timeScale = Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.Joystick1Button5) ? 0 : 1;
		if (Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick1Button4)) {
			Vector3 v1 = Vector3.zero;
			v1.x = (Input.GetKey(KeyCode.Joystick1Button4) ? 1 : -1) * prevVel.z;
			v1.y = m_Rigidbody.velocity.y;
			v1.z = (Input.GetKey(KeyCode.Joystick1Button4) ? -1 : 1) * prevVel.x;

			m_Rigidbody.velocity = v1;
			m_Rigidbody.angularVelocity = prevAngVel;
			prevVel = v1;
		}
		else {

			float wo = GetWorldOrientation();

			Debug.Log(wo);
			Debug.Log(dir);

			//only really works when original dir is true, otherwise should be the other way
			bool newdir = PlayerCharacter.instance.movingOnXAxis() ? false : true;

			if (!dir)
				newdir = !newdir;

			Vector3 v1 = Vector3.zero;
			v1.x = -prevVel.z * (newdir ? 1 : -1);
			v1.y = m_Rigidbody.velocity.y;
			v1.z = -prevVel.x * (newdir ? 1 : -1);

			m_Rigidbody.velocity = v1;
			m_Rigidbody.angularVelocity = prevAngVel;
			prevVel = v1;
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
		Animator _anim = CameraShake.instance.GetComponent<Animator>();
		_anim.Play("splash_outro");
		_anim.SetFloat("rev",-1);

		yield return new WaitForSeconds (1.25f);
		WorldGen.instance.GenerateSillyName();
		yield return new WaitForSeconds (0.23f);
		_anim.SetFloat("rev",0);
		yield return new WaitForSeconds (2f);
		while (WorldGen.instance.generating)
			yield return null;
		lerpSpeed = 2;
		yield return new WaitForSeconds (1);
		forceRotation = true;
		yield return new WaitForSeconds (1);
		GameStateManager.instance.ChangeState(GameStateManager.GameStates.STATE_GAMEPLAY);
		lerpSpeed = 6;
		GetComponent<Rigidbody>().isKinematic = false;
		forceRotation = false;
	}

	//Round camera rotation to nearest 15 degrees
	Quaternion snapRot ()
	{
		Vector3 ea = accurateRot.eulerAngles;
		return Quaternion.Euler(new Vector3 (Mathf.Round(ea.x / 15) * 15, Mathf.Round(ea.y / 5) * 5, Mathf.Round(ea.z / 15) * 15));
	}

	float ElLerpo = 0, minimapLerp = 0;
	public Vector3 idealPosition;

	public int minimapSnap;

	bool bigMap;

	float RoundToNearest90Degrees (float f)
	{
		if (f < -45)
			return -90;
		else {
			//Differences between input value and 90 chunks
			float[] _f = {
				Mathf.Abs(f),
				Mathf.Abs(90 - f),
				Mathf.Abs(180 - f),
				Mathf.Abs(270 - f),
				Mathf.Abs(360 - f)
			};

			//find the lowest one
			float lowest = 360;
			int returnMe = 0;

			for (int i = 0; i < _f.Length; i++) {
				if (_f [i] < lowest) {
					lowest = _f [i];
					returnMe = i;
				}
			}

			return returnMe * 90;
		}
	}

	[SerializeField]
	Vector3 minimapOffset;

	void Update ()
	{
		#region minimap
		if (Input.GetKeyDown(KeyCode.M)) {
			bigMap = !bigMap;
		}

		if (bigMap) {
			float v = Input.GetAxis("RSVertical");
			float h = Input.GetAxis("RSHorizontal");
			minimapOffset = new Vector3 (minimapOffset.x + (h * 5), 0, minimapOffset.z + (v * 5));
		}
		else {
			minimapOffset = Vector3.Lerp(minimapOffset,Vector3.zero,Time.deltaTime * 3);
		}

		Vector3 startPos, endPos, startScale, endScale;
		startPos = new Vector3 (14.4f, 9.6f, 0);
		endPos = Vector3.zero + (Vector3.back * 0.01f);
		startScale = new Vector3 (7.2f, 4.8f, 1);
		endScale = new Vector3 (36, 24, 1);

		minimapLerp += Time.deltaTime * (bigMap ? 1 : -1);

		if (minimapLerp > 1)
			minimapLerp = 1;
		if (minimapLerp < 0)
			minimapLerp = 0;

		miniMapQuad.transform.localPosition = Vector3.Lerp(startPos,endPos,minimapLerp);
		miniMapQuad.transform.localScale = Vector3.Lerp(startScale,endScale,minimapLerp);
		minimapCam.GetComponent<Camera>().orthographicSize = (Mathf.Round(Mathf.Lerp(64,192,minimapLerp) / 8) * 8);

		minimapCam.position = PlayerAvatar.transform.position + (Vector3.up * 137) + minimapOffset;
		minimapCam.position = new Vector3 (
			((int)minimapCam.transform.position.x / minimapSnap) * minimapSnap,
			((int)minimapCam.transform.position.y / minimapSnap) * minimapSnap,
			((int)minimapCam.transform.position.z / minimapSnap) * minimapSnap
		);
		#endregion

		if (forceRotation) {
			ElLerpo += Time.deltaTime;
			accurateRot = Quaternion.Lerp(startRot,Quaternion.Euler(Vector3.down * 180),ElLerpo);
			CameraObject.localRotation = snapRot();
		}

		#region offset adjustment

		adjustedOffset = offset;

		#region camera pan left/right
		float camRot = Mathf.Round(CameraObject.localRotation.eulerAngles.y);

		float selfRot = transform.rotation.eulerAngles.y;
		if (selfRot > 180)
			selfRot -= 360;
		if (selfRot < -180)
			selfRot += 360;
			
		if (camRot == 0 || camRot == 180) {
			//Z locked

			int adjuster = 1;
			if (selfRot > 0 && selfRot < 180)
				adjuster *= -1;

			adjustedOffset.x = offset.x * adjuster * (camRot == 180 ? -1 : 1);
		}
		else {
			//X locked

			int adjuster = 1;
			if (selfRot > 90)
				adjuster *= -1;
			if (selfRot < -90)
				adjuster *= -1;
			
			adjustedOffset.z = offset.z * adjuster;
		}
		#endregion

		int rot = Mathf.RoundToInt(CameraObject.transform.localRotation.eulerAngles.y);

		if (rot != 0 && rot != 90) {
			adjustedOffset.x *= -1;
		}
		else if (rot == -90 || rot == 0) {
				adjustedOffset.z *= -1;
			}

		#endregion

		Vector3 clampedPos = PlayerAvatar.position;
		if (clampedPos.y < -15)
			clampedPos.y = -15;
		if (rotationAmount == 0) {
			//Snap cam into offset position when stopping rotating 
			idealPosition = Vector3.Lerp(CameraObject.position,clampedPos + adjustedOffset,Time.deltaTime * lerpSpeed);
			CameraObject.position = idealPosition;
		}
		else
			idealPosition = CameraObject.position;

		if (GameStateManager.instance.GetState() == GameStateManager.GameStates.STATE_GAMEPLAY)
			RotatePerspective();
		ControllerInput();

	}

	//Rotation function used by the player
	public void Rotate (bool dir)//false left, true right
	{
		if (clockwise)
			dir = !dir;
		
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

	//Rotation function used by meeee
	public void Rotate (float doorAngle, float worldAngle)
	{
		float additionalRotation = doorAngle - worldAngle;
		if (additionalRotation == 270)
			additionalRotation = -90;
		if (additionalRotation == -270)
			additionalRotation = 90;
		rotationAmount += additionalRotation;
		idealRotation += rotationAmount;
		if (idealRotation < 0)
			idealRotation += 360;
		if (idealRotation > 360)
			idealRotation -= 360;

		Time.timeScale = rotationAmount == 0 ? 1 : 0;
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
