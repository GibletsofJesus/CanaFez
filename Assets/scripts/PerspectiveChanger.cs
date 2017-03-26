using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;

public class PerspectiveChanger : MonoBehaviour
{
	public static PerspectiveChanger instance;

	public bool clockwise;

	[SerializeField]
	Transform CameraObject, PlayerAvatar, minimapCam, WorldObjects, miniMapQuad;
	[SerializeField]
	minimapRotater minimapThing;
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
			CameraObject.RotateAround(transform.position,Vector3.up,Time.unscaledDeltaTime * changeSpeed);

			Quaternion q = Quaternion.AngleAxis(-Time.unscaledDeltaTime * changeSpeed,Vector3.up);
			//addOFf = q * addOFf;

			//Need to also rotate the player
			Vector3 e = transform.localRotation.eulerAngles;
			e.y += Time.unscaledDeltaTime * changeSpeed;
			transform.localRotation = Quaternion.Euler(e);

			if (rotationAmount < 0) {
				CameraObject.localRotation = Quaternion.Euler(new Vector3 (0, RoundToNearest90Degrees(CameraObject.localRotation.eulerAngles.y), 0));
				PlayerCharacter.instance.UpdateMovementRestirctions(CameraObject.transform.localRotation.eulerAngles.y);
				rotationAmount = 0;
				OffsetAdjustment();
				lerpSpeed *= 10;
				findIdealPositionAndSet();
				lerpSpeed /= 10;

				checkFinishRotating(true);
			}
		}
		else if (rotationAmount < 0) {
				rotationAmount += Time.unscaledDeltaTime * changeSpeed;
				CameraObject.RotateAround(PlayerAvatar.position,Vector3.down,Time.unscaledDeltaTime * changeSpeed);

				Quaternion q = Quaternion.AngleAxis(Time.unscaledDeltaTime * changeSpeed,Vector3.up);
				//addOFf = q * addOFf;

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
		Time.timeScale = Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.Joystick1Button5) ? 0 : 1;

		Quaternion q = Quaternion.AngleAxis(rotationToComplete,Vector3.up);

		m_Rigidbody.velocity = q * prevVel;

		rotationToComplete = 0;
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

	#region intro sequence

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

		PlayerCharacter.instance.transform.position = PlayerCharacter.instance.lastSpawner.transform.position + (Vector3.up * 3);

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

	#endregion

	float ElLerpo = 0, minimapLerp = 0;
	public Vector3 idealPosition;

	public int minimapSnap;
	bool bigMap;

	public float RoundToNearest90Degrees (float f)
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

	Vector3 clampedPlayerPos ()
	{
		Vector3 returnMe = PlayerAvatar.position;
		if (returnMe.y < -15)
			returnMe.y = -15;

		return returnMe;
	}

	void findIdealPositionAndSet ()
	{
		//Snap cam into offset position when stopping rotating 
		//idealPosition = Vector3.Lerp(CameraObject.position,clampedPlayerPos() + adjustedOffset + addOFf,Time.unscaledDeltaTime * lerpSpeed);
		idealPosition = Vector3.Lerp(CameraObject.position,clampedPlayerPos() + adjustedOffset,Time.unscaledDeltaTime * lerpSpeed);
		CameraObject.position = idealPosition;
	}

	float PrevH = 1;

	void OffsetAdjustment ()
	{
		adjustedOffset = offset;

		Quaternion q = Quaternion.AngleAxis(CameraObject.localRotation.eulerAngles.y,Vector3.up);

		if (GameStateManager.instance.GetState() == GameStateManager.GameStates.STATE_GAMEPLAY) {
			//Still needs to be fixed a bit
			float h = CrossPlatformInputManager.GetAxis("Horizontal");
			if (h != 0 && lerpSpeed < 10) {
				minimapThing.dir = h > 0 ? true : false;
				if (rotationAmount == 0)
					minimapThing.Snap();
				Debug.Log(minimapThing.dir);
				adjustedOffset.x *= h > 0 ? -1 : 1;
				PrevH = h;
			}
			else {
				minimapThing.dir = PrevH > 0 ? true : false;
				//minimapThing.Snap();
				adjustedOffset.x *= PrevH > 0 ? -1 : 1;
			}
		}
		adjustedOffset = q * adjustedOffset;
	}

	void Update ()
	{
		if (!instance)
			instance = this;
		
		if (forceRotation) {
			ElLerpo += Time.deltaTime;
			accurateRot = Quaternion.Lerp(startRot,Quaternion.Euler(Vector3.down * 180),ElLerpo);
			CameraObject.localRotation = snapRot();
		}

		if (GameStateManager.instance.GetState() != GameStateManager.GameStates.STATE_PAUSE) {
			OffsetAdjustment();
			findIdealPositionAndSet();
		}
		
		if (GameStateManager.instance.GetState() == GameStateManager.GameStates.STATE_GAMEPLAY) {

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

			minimapLerp += Time.deltaTime * (bigMap ? 5 : -5);

			if (minimapLerp > 1)
				minimapLerp = 1;
			if (minimapLerp < 0)
				minimapLerp = 0;

			miniMapQuad.transform.localPosition = Vector3.Lerp(startPos,endPos,minimapLerp);
			miniMapQuad.transform.localScale = Vector3.Lerp(startScale,endScale,minimapLerp);
			minimapCam.GetComponent<Camera>().orthographicSize = (Mathf.Round(Mathf.Lerp(64,192,minimapLerp) / 8) * 8);

			minimapCam.position = PlayerAvatar.transform.position + (Vector3.up * 350) + minimapOffset;
			minimapCam.position = new Vector3 (
				((int)minimapCam.transform.position.x / minimapSnap) * minimapSnap,
				((int)minimapCam.transform.position.y / minimapSnap) * minimapSnap,
				((int)minimapCam.transform.position.z / minimapSnap) * minimapSnap
			);
			#endregion

			ControllerInput();
		}
		RotatePerspective();
	}

	float rotationToComplete;
	Vector3 addOFf;


	//Rotation function used by the player
	public void Rotate (bool dir)//false left, true right
	{
		if (clockwise)
			dir = !dir;

		//addOFf = CameraObject.position - (clampedPlayerPos() + adjustedOffset);

		rotationAmount = 90 * (dir ? 1 : -1);
		minimapThing.SetRotation(rotationAmount);

		rotationToComplete = rotationAmount;
		//findIdealPositionAndSet();
		//Equivelant of pausing this ting
		prevVel = m_Rigidbody.velocity;
		prevAngVel = m_Rigidbody.angularVelocity;
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

		minimapThing.SetRotation(rotationAmount);

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
