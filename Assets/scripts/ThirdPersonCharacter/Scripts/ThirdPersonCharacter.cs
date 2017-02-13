using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class ThirdPersonCharacter : MonoBehaviour
{
	public static ThirdPersonCharacter instance;

	#region all ThreadSafeAttribute prefab shiz

	[Header("Movement stats")]
	[SerializeField] float m_MovingTurnSpeed = 360;
	[SerializeField] float m_StationaryTurnSpeed = 180;
	[SerializeField] float m_JumpPower = 12f;
	[Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
	[SerializeField] float m_RunCycleLegOffset = 0.2f;
	//specific to the character in sample assets, will need to be modified to work with others
	[SerializeField] float m_MoveSpeedMultiplier = 1f, maxSpeedMultiplier;
	[SerializeField] float m_AnimSpeedMultiplier = 1f;
	[SerializeField] float m_GroundCheckDistance = 0.1f;

	Rigidbody m_Rigidbody;
	Animator m_Animator;
	bool m_IsGrounded, m_Crouching;
	const float k_Half = 0.5f;
	float m_TurnAmount, m_ForwardAmount, m_CapsuleHeight, m_OrigGroundCheckDistance;
	bool running;
	Vector3 m_GroundNormal, m_CapsuleCenter;
	CapsuleCollider m_Capsule;

	#endregion

	[Header("Upgrades")]
	[SerializeField]
	bool doubleJump;
	bool extraJump;

	[Header("Audio")]
	[SerializeField]
	AudioClip[] footsteps;
	[SerializeField]
	AudioClip stompLandingSound;

	[Header("All the other shit")]
	[SerializeField]
	Transform startingPosition;

	[SerializeField]
	Transform[] footsies;
	bool[] footGrounded = { true, true };
	[SerializeField]
	ParticleSystem[] footDust;

	[HideInInspector]
	public Animator lastSpawner;

	void Start ()
	{
		instance = this;
		m_Animator = GetComponent<Animator>();
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Capsule = GetComponent<CapsuleCollider>();
		m_CapsuleHeight = m_Capsule.height;
		m_CapsuleCenter = m_Capsule.center;

		m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ;
		m_OrigGroundCheckDistance = m_GroundCheckDistance;
	}

	void CheckDeath ()
	{
		if (transform.position.y < -17 && !respawning) {
			StartCoroutine(respawnEffect());
		}
	}

	public bool respawning;

	IEnumerator respawnEffect ()
	{
		respawning = true;

		transform.position = lastSpawner.transform.position;
		m_Rigidbody.velocity = Vector3.zero;
		m_Rigidbody.angularVelocity = Vector3.zero;

		PerspectiveChanger pc = GetComponent<PerspectiveChanger>();
		while (Vector3.Distance(transform.position + pc.offset,GetComponent<PerspectiveChanger>().idealPosition) > 19f) {
			//Debug.Log("waiting for cam");
			yield return new WaitForEndOfFrame ();
		}
		transform.rotation = Quaternion.Euler(Vector3.up * 90f);//* (lastSpawner.transform.localRotation.y > 0 ? 1f : -1f));

		float thing = lastSpawner.transform.localRotation.eulerAngles.y - PerspectiveChanger.instance.GetWorldOrientation();

		if (Mathf.Abs(thing) != 180)
			PerspectiveChanger.instance.Rotate(thing);

		lastSpawner.Play("rooftopDoor_open");
		yield return new WaitForSeconds (1);
		//Debug.Log("starting 1 sec wait");
		//Move forward a little bit
		//Then return control to player
		respawning = false;
	}

	public void Move (Vector3 move, bool crouch, bool jump)
	{
		if (respawning && lastSpawner.GetCurrentAnimatorStateInfo(0).IsName("rooftopDoor_open")) {
			move = Vector3.left * 0.7f * (lastSpawner.transform.localRotation.y > 0 ? -1f : 1f);
			crouch = false;
			jump = false;
		}
		else if (respawning) {
				move = Vector3.zero;
			}
		CheckDeath();
		if (CrossPlatformInputManager.GetButtonDown("Fire1")) {
			//Do a reset
			transform.position = lastSpawner.transform.position;
			m_Rigidbody.velocity = Vector3.zero;
			m_Rigidbody.angularVelocity = Vector3.zero;
		}

		// convert the world relative moveInput vector into a local-relative
		// turn amount and forward amount required to head in the desired
		// direction.
		if (move.magnitude > 1f)
			move.Normalize();
		move = transform.InverseTransformDirection(move);

		//Slight extra bit of acceleration
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

		if (Mathf.Abs(m_Rigidbody.velocity.x) > 6)
			checkFootCollision();
		CheckGroundStatus();
		move = Vector3.ProjectOnPlane(move,m_GroundNormal);
		m_TurnAmount = Mathf.Atan2(move.x,move.z);
		m_ForwardAmount = move.z;

		ApplyExtraTurnRotation();

		// control and velocity handling is different when grounded and airborne:
		if (m_IsGrounded) {//|| extraJump) {
			HandleGroundedMovement(crouch,jump);
		}
		else {
			HandleAirborneMovement();
		}

		//ScaleCapsuleForCrouching(crouch);

		// send input and other state parameters to the animator
		UpdateAnimator(move);
	}

	IEnumerator CrouchRoll ()
	{
		//For normal jumps (i.e. no powerups) don't make a crater
		//Instead, if player lands at a lower position than that they jumped from, do a roll

		//Make sure animator is in the crouching state
		//Rotate player avatar at the waistline in local z axis
		//When rotated, set animator to standing and do a resume

		//Need to make sure velocity is preserved during this time of crouching.
		yield return new WaitForEndOfFrame ();
	}

	void UpdateAnimator (Vector3 move)
	{
		// update the animator parameters
		m_Animator.SetFloat("Forward",m_ForwardAmount,0.1f,Time.deltaTime);
		m_Animator.SetFloat("Turn",m_TurnAmount,0.1f,Time.deltaTime);
		m_Animator.SetBool("Crouch",m_Crouching);
		m_Animator.SetBool("OnGround",m_IsGrounded);
		if (!m_IsGrounded) {
			m_Animator.SetFloat("Jump",-4);
			//m_Animator.SetFloat("Jump",m_Rigidbody.velocity.y);
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

	public float amp, maxStrafeSpeed = 20;

	void HandleAirborneMovement ()
	{
		//Allow strafing
		float h = -CrossPlatformInputManager.GetAxis("Horizontal");

		Vector3 vel = m_Rigidbody.velocity;

		if ((vel.x < maxStrafeSpeed && h > 0) || (vel.x > -maxStrafeSpeed && h < 0)) {
			vel = new Vector3 (vel.x + (h * Time.deltaTime * amp), vel.y, vel.z);
			if (vel.x > maxStrafeSpeed)
				vel.x = maxStrafeSpeed;
			if (vel.x < -maxStrafeSpeed)
				vel.x = -maxStrafeSpeed;
		}
		m_Rigidbody.velocity = vel;

		if (CrossPlatformInputManager.GetButtonDown("Jump") && extraJump) {
			extraJump = false;
			m_Rigidbody.velocity = new Vector3 (m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
		}
		// apply extra gravity from multiplier:
		Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
		m_Rigidbody.AddForce(extraGravityForce);

		m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
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

	#region everything to with landing / being on the ground

	void HandleGroundedMovement (bool crouch, bool jump)
	{			
		// check whether conditions are right to allow a jump:
		if (jump && !crouch && (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded") || extraJump)) {
			// jump!				
			m_Rigidbody.velocity = new Vector3 (m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
			m_IsGrounded = false;
			m_Animator.applyRootMotion = true;
			m_GroundCheckDistance = 0.1f;
		}
	}

	[SerializeField]
	float dustDistance;

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

	float vertSpeed;

	void CheckGroundStatus ()
	{
		RaycastHit hitInfo;
#if UNITY_EDITOR
		// helper to visualise the ground check ray in the scene view
		Debug.DrawLine(transform.position + (Vector3.up * 0.1f),transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
		// 0.1f is a small offset to start the ray from inside the character
		// it is also good to note that the transform position in the sample assets is at the base of the character
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f),Vector3.down,out hitInfo,m_GroundCheckDistance)) {
			m_GroundNormal = hitInfo.normal;
			if (!m_IsGrounded)
				landing(hitInfo.point);
			m_IsGrounded = true;
			m_Animator.applyRootMotion = true;
		}
		else {
			m_IsGrounded = false;
			m_GroundNormal = Vector3.up;
			m_Animator.applyRootMotion = true;
		}
		decreaseVibration();
	}

	float vibration;

	void decreaseVibration ()
	{
		if (vibration > 0) {
			vibration -= Time.deltaTime;
			GamePad.SetVibration(0,vibration,vibration);
		}
		
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

	#endregion

	void landing (Vector3 position)
	{		
		if (doubleJump) {			
			extraJump = true;
		}

		if (allowCraters && Mathf.Abs(m_Rigidbody.velocity.y) > craterSpeed) {
			//Make a cater effect
			vibration = 0.5f;
			CameraShake.instance.shakeDuration += 0.25f;
			CameraShake.instance.shakeAmount = .5f; 
			SoundManager.instance.playSound(stompLandingSound,1,Random.Range(0.9f,1.1f));
			CraterManager.instance.SpawnCrater(position + (Vector3.up * Random.Range(0.005f,0.025f)));
			//CraterManager.instance.SpawnCrater(transform.position + Vector3.up * 0.1f);
			footDust [2].transform.position = transform.position + (Vector3.up * dustDistance);
			footDust [2].Emit(30);
		}
		else {
			footDust [2].transform.position = transform.position + (Vector3.up * dustDistance);
			footDust [2].Emit(15);
		}

		m_MoveSpeedMultiplier = 1 + ((m_MoveSpeedMultiplier - 1) / 2);
	}

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