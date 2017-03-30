using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacter : MonoBehaviour
{
	public static PlayerCharacter instance;

	void Update ()
	{
		if (!instance)
			instance = this;
	}

	#region all the prefab shiz

	float m_MovingTurnSpeed = 480;
	float m_StationaryTurnSpeed = 720;
	float m_JumpPower = 10f;
	float m_GravityMultiplier = 2f;
	float m_RunCycleLegOffset = 0.2f;
	//specific to the character in sample assets, will need to be modified to work with others
	float m_MoveSpeedMultiplier = 1.2f, maxSpeedMultiplier = 4;
	float m_AnimSpeedMultiplier = 1.2f;
	float m_GroundCheckDistance = 0.3f;

	[Header("Movement/state")]
	public bool respawning;
	//Vertical speed modifiers
	[SerializeField]
	float newMaxVertSpeed, speedAmp;
	[HideInInspector]
	public Animator lastSpawner;

	Rigidbody m_Rigidbody;
	Animator m_Animator;
	[SerializeField]
	bool m_IsGrounded, m_Crouching;
	const float k_Half = 0.5f;
	float m_TurnAmount, m_ForwardAmount, m_CapsuleHeight, m_OrigGroundCheckDistance;
	bool running;
	Vector3 m_GroundNormal, m_CapsuleCenter;
	CapsuleCollider m_Capsule;

	#endregion

	[SerializeField]
	Transform startingPosition;
	[Header("Jumping")]
	[SerializeField]
	Transform jumpBar;
	SpriteRenderer jumpBarSprite;
	float jumpTimer = 0;
	Vector3 jumpStartPosition;
	[HideInInspector]
	public float airbourneSpeedMultiplier = 3, maxStrafeSpeed = 15, maxJumpTimer = 2.5f;

	[Header("Rolling related")]
	public bool rolling;
	[SerializeField]
	Transform waistline, rollRotationObject;
	[SerializeField]
	float minRollSpeed;
	//The minimum speed required to do an roll

	[Header("Audio")]
	[SerializeField]
	AudioClip[] footsteps;
	[SerializeField]
	AudioClip stompLandingSound, deathSound, doorOpen, doorClose;

	[Header("Cool FX")]
	[SerializeField]
	Transform[] footsies;
	bool[] footGrounded = { true, true };
	[SerializeField]
	ParticleSystem[] footDust;
	float runningFXSpeed = 5, vibration;
	[SerializeField]
	ParticleSystem speedLines;

	void Start ()
	{
		instance = this;
		m_Animator = GetComponent<Animator>();
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Capsule = GetComponent<CapsuleCollider>();
		m_CapsuleHeight = m_Capsule.height;
		m_CapsuleCenter = m_Capsule.center;
		jumpStartPosition = Vector3.down * 100;

		m_Rigidbody.constraints =
		RigidbodyConstraints.FreezeRotationX |
		RigidbodyConstraints.FreezeRotationY |
		RigidbodyConstraints.FreezePositionZ |
		RigidbodyConstraints.FreezeRotationZ;
		
		m_OrigGroundCheckDistance = m_GroundCheckDistance;
	}

	void CheckDeath ()
	{
		if (transform.position.y < -17 && !respawning) {
			StartCoroutine(respawnEffect());
		}
	}

	IEnumerator respawnEffect ()
	{
		respawning = true;
		SoundManager.instance.playSound(deathSound);
		yield return new WaitForSeconds (1);
		m_Animator.SetBool("OnGround",m_IsGrounded);
		m_Animator.SetBool("Crouch",false);
		m_IsGrounded = true;
		m_Rigidbody.velocity = Vector3.zero;
		m_Rigidbody.angularVelocity = Vector3.zero;
		transform.position = lastSpawner.transform.position;
		m_Rigidbody.isKinematic = false;
		//yield return new WaitForSeconds (.5f);

		Quaternion q = Quaternion.AngleAxis(PerspectiveChanger.instance.GetWorldOrientation(),Vector3.up);
		while (Vector3.Distance(transform.position + (q * PerspectiveChanger.instance.offset),PerspectiveChanger.instance.idealPosition) > 19f) {
			yield return new WaitForEndOfFrame ();
		}
		PerspectiveChanger.instance.Rotate(lastSpawner.transform.localRotation.eulerAngles.y,PerspectiveChanger.instance.GetWorldOrientation());

		SoundManager.instance.playSound(doorOpen);
		lastSpawner.Play("rooftopDoor_open");
		yield return new WaitForSeconds (1);
		respawning = false;
		yield return new WaitForSeconds (0.45f);
		SoundManager.instance.playSound(doorClose);
		//Move forward a little bit
		//Then return control to player
	}

	void UpdateAnimator (Vector3 move)
	{
		// update the animator parameters
		m_Animator.SetFloat("Forward",m_ForwardAmount,0.1f,Time.deltaTime);
		m_Animator.SetFloat("Turn",m_TurnAmount,0.1f,Time.deltaTime);
		m_Animator.SetBool("Crouch",m_Crouching);
		m_Animator.SetBool("OnGround",m_IsGrounded);

		if (!m_IsGrounded) {
			m_Animator.SetFloat("Jump",m_Rigidbody.velocity.y);
		}

		// calculate which leg is behind, so as to leave that leg trailing in the jump animation
		// (This code is reliant on the specific run cycle offset in our animations,
		// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
		float runCycle =
			Mathf.Repeat(
				m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset,1);
		float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
		if (m_IsGrounded) {
			m_Animator.SetFloat("JumpLeg",jumpLeg);
		}

		// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
		// which affects the movement speed because of the root motion.
		if (m_IsGrounded && move.magnitude > 0) {
			m_Animator.speed = m_AnimSpeedMultiplier;
		}
		else {
			// don't use that while airborne
			m_Animator.speed = 1;
		}
	}

	public void UnPause ()
	{
		SoundManager.instance.managedAudioSources [0].AudioSrc.volume = SoundManager.instance.managedAudioSources [0].volumeLimit;
		m_Rigidbody.velocity = UnPausedVelocity;
		m_Rigidbody.isKinematic = false;
	}

	Vector3 UnPausedVelocity;

	public void Pause ()
	{
		SoundManager.instance.managedAudioSources [0].AudioSrc.volume = 0;
		UnPausedVelocity = m_Rigidbody.velocity;
		m_Rigidbody.isKinematic = true;
	}

	#region movement

	[SerializeField]
	Color[] basicColors;
	int flashIndex = 0;
	[SerializeField]
	AudioClip sound;
	[SerializeField]
	float[] pitchMod;

	public void Move (Vector3 move, bool crouch)
	{
		#region Stop the player from moving along another axis
		if (movingOnXAxis()) {
			move.z = 0;
		}
		else
			move.x = 0;
		#endregion

		#region big yumps

		bool jump = false;
		if (CrossPlatformInputManager.GetButton("Jump") && m_IsGrounded && jumpTimer <= maxJumpTimer) {
			SoundManager.instance.managedAudioSources [1].volumeLimit = Mathf.Lerp(0.1f,0.5f,jumpTimer / maxJumpTimer);
			SoundManager.instance.managedAudioSources [1].AudioSrc.pitch = Mathf.Lerp(pitchMod [0],pitchMod [1],jumpTimer / maxJumpTimer);
			SoundManager.instance.managedAudioSources [1].AudioSrc.volume = SoundManager.instance.managedAudioSources [1].volumeLimit;

			jumpTimer += Time.deltaTime;
			if (jumpTimer > maxJumpTimer) {
				//m_IsGrounded = false;

				//Don't force a jump out
				//jump = true;

				SoundManager.instance.managedAudioSources [1].AudioSrc.pitch = Mathf.Lerp(pitchMod [0],pitchMod [1] + ((float)flashIndex / 100f),jumpTimer / maxJumpTimer);
				jumpTimer = maxJumpTimer;
				if (!jumpBarSprite)
					jumpBarSprite = jumpBar.GetComponentInChildren<SpriteRenderer>();

				flashIndex++;
				if (flashIndex > basicColors.Length - 1) {
					flashIndex = 0;
				}
				jumpBarSprite.color = basicColors [flashIndex];
			}
			jumpBar.localScale = new Vector3 (Mathf.Lerp(0,61,jumpTimer / maxJumpTimer), 1, 1);
		}
		else
			jumpBar.localScale = new Vector3 (Mathf.Lerp(jumpBar.transform.localScale.x,0,Time.deltaTime * 3), 1, 1);
		
		if (!CrossPlatformInputManager.GetButton("Jump") && m_IsGrounded && jumpTimer > 0) {
			SoundManager.instance.playSound(sound,1,pitchMod [2]);

			SoundManager.instance.managedAudioSources [1].volumeLimit = 0;
			SoundManager.instance.managedAudioSources [1].AudioSrc.volume = SoundManager.instance.managedAudioSources [1].volumeLimit;
			flashIndex = 0;
			if (jumpBarSprite)
				jumpBarSprite.color = Color.white;
			jump = true;
		}
		#endregion

		#region walk out of the respawn area
		if (respawning && lastSpawner.GetCurrentAnimatorStateInfo(0).IsName("rooftopDoor_open")) {
			bool dir = lastSpawner.transform.localRotation.eulerAngles.y < 0;

			dir = (int)lastSpawner.transform.localRotation.eulerAngles.y >= Mathf.RoundToInt(PerspectiveChanger.instance.GetWorldOrientation());
			//dir = (int)lastSpawner.transform.localRotation.eulerAngles.y == 90 && Mathf.RoundToInt(PerspectiveChanger.instance.GetWorldOrientation()) == 90 ? false : dir;

			move = lastSpawner.transform.right * 0.7f * (dir ? -1f : 1f);
			crouch = false;
		}
		else if (respawning) {
				move = Vector3.zero;
			}
		#endregion

		#region normal movement...?
		if (!rolling) {
			CheckDeath();
			if (CrossPlatformInputManager.GetButtonDown("Fire1")) {
				m_Rigidbody.isKinematic = true;
				StartCoroutine(respawnEffect());
			}

			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f)
				move.Normalize();
			move = transform.InverseTransformDirection(move);

			//Slight extra bit of acceleration
			if (!respawning) {
				if (Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) > .75f) {
					running = true;
				}
				else {
					m_MoveSpeedMultiplier = 1 + Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) / 4f;
					running = false;
				}
				if (running && m_MoveSpeedMultiplier < maxSpeedMultiplier && m_IsGrounded) {
					m_MoveSpeedMultiplier += Time.fixedDeltaTime;
					if (m_MoveSpeedMultiplier > maxSpeedMultiplier)
						m_MoveSpeedMultiplier = maxSpeedMultiplier;
				}
				//Dust poofs on the feet for running fast
				if (Mathf.Abs(m_Rigidbody.velocity.magnitude) > runningFXSpeed)
					checkFootCollision();
			}
			CheckGroundStatus();
			move = Vector3.ProjectOnPlane(move,m_GroundNormal);
			m_TurnAmount = Mathf.Atan2(move.x,move.z);
			m_ForwardAmount = move.z;

			// control and velocity handling is different when grounded and airborne:
			if (m_IsGrounded) {
				ApplyExtraTurnRotation();
				HandleGroundedMovement(crouch,jump);
			}
			else {
				HandleAirborneMovement();
			}
			UpdateAnimator(move);
		}
		#endregion

		#region Wooshing effects
		if (GameStateManager.instance.GetState() == GameStateManager.GameStates.STATE_GAMEPLAY) {
			SoundManager.instance.managedAudioSources [0].volumeLimit = (Mathf.Lerp(
				SoundManager.instance.managedAudioSources [0].volumeLimit,
				(m_Rigidbody.velocity.magnitude * m_Rigidbody.velocity.magnitude) / 750f,
				Time.deltaTime * 5)
			/ 1f) * SoundManager.instance.volumeMultiplayer;
			SoundManager.instance.managedAudioSources [0].AudioSrc.pitch = Mathf.Clamp(0.1f + Mathf.Lerp(SoundManager.instance.managedAudioSources [0].AudioSrc.pitch,
				(m_Rigidbody.velocity.magnitude * m_Rigidbody.velocity.magnitude) / 1250f,
				Time.deltaTime * 5),0,2);
			SoundManager.instance.managedAudioSources [0].AudioSrc.volume = SoundManager.instance.managedAudioSources [0].volumeLimit;

			ParticleSystem.EmissionModule em = speedLines.emission;
		
			em.rateOverDistance = (m_Rigidbody.velocity.sqrMagnitude - 200f) / 150f;
		}
		else {
			SoundManager.instance.managedAudioSources [0].AudioSrc.volume = 0;			
		}
		#endregion
	}

	float prevVertVel;

	void HandleAirborneMovement ()
	{
		//Allow strafing
		float h = CrossPlatformInputManager.GetAxis("Horizontal");

		if (PerspectiveChanger.instance.GetWorldOrientation() > 130)
			h *= -1;

		Vector3 vel = m_Rigidbody.velocity;

		//Reverse the velocity added if we're facing certain directions
		if (!movingOnXAxis()) {
			if ((vel.z < maxStrafeSpeed && h > 0) || (vel.z > -maxStrafeSpeed && h < 0)) {
				vel = new Vector3 (vel.x, vel.y, vel.z - (h * Time.deltaTime * airbourneSpeedMultiplier));
				if (vel.z > maxStrafeSpeed)
					vel.z = maxStrafeSpeed;
				if (vel.z < -maxStrafeSpeed)
					vel.z = -maxStrafeSpeed;
			}
		}
		else {
			if ((vel.x < maxStrafeSpeed && h > 0) || (vel.x > -maxStrafeSpeed && h < 0)) {
				vel = new Vector3 (vel.x + (h * Time.deltaTime * airbourneSpeedMultiplier), vel.y, vel.z);
				if (vel.x > maxStrafeSpeed)
					vel.x = maxStrafeSpeed;
				if (vel.x < -maxStrafeSpeed)
					vel.x = -maxStrafeSpeed;
			}
		}
		//If we're going down
		if (vel.y < 0 && vel.y > -newMaxVertSpeed) {
			vel.y -= Time.deltaTime * speedAmp;
		}
		m_Rigidbody.velocity = vel;

		if (vel.y < 0)
			prevVertVel = m_Rigidbody.velocity.y;

		// apply extra gravity from multiplier:
		Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
		m_Rigidbody.AddForce(extraGravityForce);

		m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.1f;
	}

	public void OnAnimatorMove ()
	{
		// we implement this function to override the default root motion.
		// this allows us to modify the positional speed before it's applied.
		if (m_IsGrounded && Time.deltaTime > 0) {
			Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

			// we preserve the existing y part of the current velocity.
			v.y = m_Rigidbody.velocity.y;
			m_Rigidbody.velocity = v;
		}
	}

	public bool movingOnXAxis ()
	{
		return Mathf.Round(CameraShake.instance.transform.localRotation.eulerAngles.y) == 0 ||
		Mathf.Round(CameraShake.instance.transform.localRotation.eulerAngles.y) == 180;
	}

	RigidbodyConstraints AxisLock ()
	{
		bool b = Mathf.Round(CameraShake.instance.transform.localRotation.eulerAngles.y) == 0 ||
		         Mathf.Round(CameraShake.instance.transform.localRotation.eulerAngles.y) == 180;

		return b ? RigidbodyConstraints.FreezePositionZ : RigidbodyConstraints.FreezePositionX; 
	}

	public void UpdateMovementRestirctions (float cameraYAngle)
	{
		if (Mathf.RoundToInt(cameraYAngle) == 180 || Mathf.RoundToInt(cameraYAngle) == 0) {
			//Before we do this, make sure to change rigidbody velocities 
			m_Rigidbody.constraints =
				RigidbodyConstraints.FreezeRotationX |
			RigidbodyConstraints.FreezeRotationY |
			RigidbodyConstraints.FreezePositionZ |
			RigidbodyConstraints.FreezeRotationZ;
		}
		else {
			m_Rigidbody.constraints =
			RigidbodyConstraints.FreezeRotationX |
			RigidbodyConstraints.FreezeRotationY |
			RigidbodyConstraints.FreezePositionX |
			RigidbodyConstraints.FreezeRotationZ;
		}
	}

	public bool lowGravity;

	void HandleGroundedMovement (bool crouch, bool jump)
	{			
		// check whether conditions are right to allow a jump and if so, do a jump.
		if (jump && !crouch && (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))) {
			
			m_Rigidbody.velocity = new Vector3 (m_Rigidbody.velocity.x, (m_JumpPower + (jumpTimer * 5)) * (lowGravity ? 1.5f : 1), m_Rigidbody.velocity.z);

			jumpStartPosition = transform.position;
			m_IsGrounded = false;
			m_Animator.applyRootMotion = false;
			m_GroundCheckDistance = 0.1f;
		}
	}

	#region everything to with landing / being on the ground

	[SerializeField]
	GameObject SpecialObject;

	IEnumerator CrouchRoll (float speed, Vector3 position)
	{
		Vector3 startRot = rollRotationObject.localRotation.eulerAngles;
		transform.position += Vector3.down * 0.1f;
		m_Rigidbody.constraints = RigidbodyConstraints.FreezePositionY;

		GetComponent<CapsuleCollider>().enabled = false;
		//Make sure animator is in the crouching state

		Vector3 forwardDir = SpecialObject.transform.forward;
		float lerpy = 0;
		while (lerpy < 1) {
			m_Animator.SetBool("OnGround",true);
			m_Animator.SetBool("Crouch",true);
			lerpy += Time.deltaTime * 2;
			rollRotationObject.RotateAround(waistline.transform.position,rollRotationObject.right,Time.deltaTime * 360 * 2);
			//Need to take gravity into account, as well as player input to slow down

			//This needs to take player rotation into account.
			//and this isn't the way of doing it.
			transform.Translate(forwardDir * speed * Time.deltaTime,Space.World);

			float h = CrossPlatformInputManager.GetAxis("Horizontal");
			#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			if (Physics.Raycast(transform.position + (Vector3.up * 0.5f),Vector3.down,2.0f)) {
				Debug.DrawLine(transform.position + (Vector3.up * 0.5f),transform.position + (Vector3.up * 0.5f) + (Vector3.down * .5f),Color.red,3f);
			}
			else {
				Debug.DrawLine(transform.position + (Vector3.up * 0.5f),transform.position + (Vector3.up * 0.5f) + (Vector3.down * .5f),Color.blue,3f);
				transform.Translate(Physics.gravity * Time.deltaTime,Space.World);
			}
			#endif

			Debug.DrawLine(transform.position,transform.position + forwardDir,Color.green,15f);
			//XOR
			if (h >= 0 && speed >= 0 || h < 0 && speed < 0)
				transform.Translate(-forwardDir * h * 5 * Time.deltaTime,Space.World);
			
			yield return new WaitForEndOfFrame ();
		}
		//Rotate player avatar at the waistline in local z axis
		//When rotated, set animator to standing and do a resume

		//Need to make sure velocity is preserved during this time of crouching.
		m_Animator.SetBool("Crouch",false);
		RaycastHit _hitInfo;
		m_Animator.SetBool("OnGround",Physics.Raycast(transform.position + (Vector3.up * 0.5f),Vector3.down,3.0f));

		m_Rigidbody.constraints =
			AxisLock() |
		RigidbodyConstraints.FreezeRotationX |
		RigidbodyConstraints.FreezeRotationY |
		RigidbodyConstraints.FreezeRotationZ;

		//transform.position = new Vector3 (transform.position.x, position.y, transform.position.z);
		rollRotationObject.localRotation = Quaternion.Euler(startRot);
		GetComponent<CapsuleCollider>().enabled = true;
		jumpStartPosition = Vector3.down * 100;
		rolling = false;
	}

	void checkFootCollision ()
	{
		RaycastHit hitInfo;
		for (int i = 0; i < 2; i++) {
			if (Physics.Raycast(footsies [i].position,Vector3.down,out hitInfo,0.1f)) {
				if (!footGrounded [i]) {
					SoundManager.instance.playSound(footsteps [Random.Range(0,footsteps.Length - 1)],0.5f,1);
					footDust [i].transform.position = footsies [i].position + (Vector3.up * 0.1f);
					footDust [i].Emit(1);
					footGrounded [i] = true;
				}
			}
			else {
				footGrounded [i] = false;
			}
		}
	}

	void CheckGroundStatus ()
	{
		RaycastHit hitInfo;
		#if UNITY_EDITOR
		// helper to visualise the ground check ray in the scene view
		Debug.DrawLine(transform.position + (Vector3.up * 0.1f),transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance),Color.red,5f);
		#endif
		// 0.1f is a small offset to start the ray from inside the character
		// it is also good to note that the transform position in the sample assets is at the base of the character
		//if (Physics.Raycast(transform.position + (Vector3.up * 0.1f),Vector3.down,out hitInfo,m_GroundCheckDistance)) {
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f),Vector3.down,out hitInfo,m_GroundCheckDistance)) {
			m_GroundNormal = hitInfo.normal;
			if (!m_IsGrounded) {
				landing(hitInfo.point);
			}
			m_IsGrounded = true;
			m_Animator.applyRootMotion = false;
		}
		else {
			m_IsGrounded = false;
			m_GroundNormal = Vector3.up;
			m_Animator.applyRootMotion = false;
		}
		decreaseVibration();
	}

	void decreaseVibration ()
	{
		if (vibration > 0) {
			vibration -= Time.deltaTime;
			GamePad.SetVibration(0,vibration,vibration);
		}

	}

	public bool isGrounded ()
	{
		return m_IsGrounded;
	}

	#region Crater stuff

	[SerializeField]
	private bool allowCraters;
	[SerializeField]
	float craterSpeed;

	public void SetCraters (bool b)
	{
		allowCraters = b;
	}

	void DoACrater (Vector3 _pos)
	{
		vibration = 0.5f;
		CameraShake.instance.shakeDuration += 0.25f;
		CameraShake.instance.shakeAmount = .5f; 
		SoundManager.instance.playSound(stompLandingSound,1,Random.Range(0.9f,1.1f));
		CraterManager.instance.SpawnCrater(_pos + (Vector3.up * 2)); //used to be up + Random.Range(0.005f,0.025f)
		footDust [2].transform.position = transform.position + (Vector3.up * 0.2f);
		footDust [2].Emit(30);
	}

	#endregion

	void landing (Vector3 position)
	{		
		#if UNITY_EDITOR
		Debug.DrawLine(position - Vector3.forward,position + Vector3.forward,Color.blue,5f);
		Debug.DrawLine(position - Vector3.left,position + Vector3.left,Color.blue,5f);
		#endif

		jumpTimer = 0;
		bool upToSpeed = Mathf.Abs(m_Rigidbody.velocity.x) > minRollSpeed || Mathf.Abs(m_Rigidbody.velocity.z) > minRollSpeed;

		if (Mathf.Abs(prevVertVel) > craterSpeed && transform.position.y > -15) {
			//Make a cater effect
			DoACrater(position);
		}
		else if (position.y < jumpStartPosition.y && upToSpeed) {
				if (!rolling && !respawning && transform.position.y > -15) {
					rolling = true;
					StartCoroutine(CrouchRoll(movingOnXAxis() ? Mathf.Abs(m_Rigidbody.velocity.x) : Mathf.Abs(m_Rigidbody.velocity.z),position));
				}
			}
			else {
				footDust [2].transform.position = transform.position + (Vector3.up * 0.2f);
				footDust [2].Emit(15);
			}

		m_MoveSpeedMultiplier = 1 + ((m_MoveSpeedMultiplier - 1) / 2);
		prevVertVel = 0;
	}

	#endregion

	#endregion

	#region probs redundant stuff

	void ScaleCapsuleForCrouching (bool crouch)
	{
		if (m_IsGrounded && crouch) {
			if (m_Crouching)
				return;
			m_Capsule.height = m_Capsule.height / 2f;
			m_Capsule.center = m_Capsule.center / 2f;
			m_Crouching = true;
		}
		else {
			Ray crouchRay = new Ray (m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
			float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
			if (Physics.SphereCast(crouchRay,m_Capsule.radius * k_Half,crouchRayLength,Physics.AllLayers,QueryTriggerInteraction.Ignore)) {
				m_Crouching = true;
				return;
			}
			m_Capsule.height = m_CapsuleHeight;
			m_Capsule.center = m_CapsuleCenter;
			m_Crouching = false;
		}
	}

	void ApplyExtraTurnRotation ()
	{
		// help the character turn faster (this is in addition to root rotation in the animation)
		float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed,m_MovingTurnSpeed,m_ForwardAmount);
		transform.Rotate(0,m_TurnAmount * turnSpeed * Time.deltaTime,0);
	}

	#endregion
}