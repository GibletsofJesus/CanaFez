﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class UpgradeManager : MonoBehaviour
{
	public static UpgradeManager instance;

	[SerializeField]
	Animator header;
	[SerializeField]
	Image pointer;
	public List<Upgrade> availableUpgrades = new List<Upgrade> ();
	[SerializeField]
	Animator[] optionAnimators;
	[SerializeField]
	Text[] upgradeText;
	[SerializeField]
	AudioClip selectionSound, switchSound;
	public List<Animator> AllZones = new List<Animator> ();
	public GameObject upgradeCanvas;

	Upgrade[] options;

	public enum upgradeType
	{
		additionalTime,
		additionalJumpTime,
		lessGravity,
		fasterStrafe,
		moreJumpPower,
		fasterRunning
		//homingLanding
	}

	public class Upgrade
	{
		public upgradeType type;

		public void Activate ()
		{
			switch (type) {
				case upgradeType.additionalJumpTime:
					//Max jump timer + 20% from base
					PlayerCharacter.instance.maxJumpTimer += 0.5f;
					break;
				case upgradeType.additionalTime:
					GameTimer.instance.timeElapsed -= 40;
					break;
				case upgradeType.lessGravity:
					Vector3 grav = Physics.gravity;
					grav.y *= 0.8f;
					Physics.gravity = grav;
					break;
				case upgradeType.fasterStrafe:
					//Max jump timer + 20% from base
					PlayerCharacter.instance.maxStrafeSpeed *= 1.5f;
					break;
				case upgradeType.moreJumpPower:
					//Max jump timer + 20% from base
					PlayerCharacter.instance.jumpAmp *= 1.25f;
					break;
				case upgradeType.fasterRunning:
					PlayerCharacter.instance.maxSpeedMultiplier *= 1.5f;
					break;
			/*case upgradeType.homingLanding:
					//SOME DAY
					break;*/
			}
		}
	}

	// Use this for initialization
	void Start ()
	{
		foreach (upgradeType ut in System.Enum.GetValues(typeof(upgradeType))) {
			availableUpgrades.Add(new Upgrade{ type = ut });
		}

		instance = this;
	}

	bool allowInput;

	void Update ()
	{
		if (!instance)
			instance = this;

		if (allowInput)
			RegisterInput();
	}

	public IEnumerator SetupUI () //and also I guess to all the upgrade shit
	{
		yield return new WaitForEndOfFrame ();
		selectionIndex = 1;
		allowInput = true;
		//Number of upgrades to choose from the available pool
		options = new Upgrade[availableUpgrades.Count > optionAnimators.Length - 1 ? optionAnimators.Length : availableUpgrades.Count];
		pointer.rectTransform.anchoredPosition = new Vector2 (40 - ((options.Length - 1) * 40) / 2, 12);

		for (int i = 0; i < options.Length; i++) {
			optionAnimators [i].GetComponent<RectTransform>().anchoredPosition =
				new Vector2 ((40 * i) - (((options.Length - 1) * 40) / 2), -20);
			optionAnimators [i].Play("upgrade_up");

			int r = Random.Range(0,availableUpgrades.Count);

			options [i] = availableUpgrades [r];
			availableUpgrades.RemoveAt(r);

			switch (options [i].type) {
				case upgradeType.additionalJumpTime:
					upgradeText [i].text = "+0.5 seconds to jump charge time";
					break;
				case upgradeType.additionalTime:
					upgradeText [i].text = "+40 seconds to game timer";
					break;
				case upgradeType.lessGravity:
					upgradeText [i].text = "-20% gravity";
					break;
				case upgradeType.fasterRunning:
					upgradeText [i].text = "+50% to max running speed";
					break;
				case upgradeType.fasterStrafe:
					upgradeText [i].text = "+50% strafing speed";
					break;
				case upgradeType.moreJumpPower:
					upgradeText [i].text = "+25% power to jumps";
					break;
			/*case upgradeType.homingLanding:
					upgradeText [i].text = "";
					break;*/
			}
		}

		header.Play("city intro");
		header.GetComponentInChildren<Text>().text = "Choose upgrade";
		yield return new WaitForSeconds (0.5f);
		header.speed = 0;
		header.playbackTime = 1.5f;

		//Fire button for selection
		while (!Input.GetButtonDown("Jump")) {
			yield return null;
		}
		allowInput = false;
		//Don't do this for all, only the active ones (might be only 2)
		for (int i = 0; i < options.Length; i++) {
			if (i != selectionIndex)
				availableUpgrades.Add(options [i]);
			optionAnimators [i].Play(i == selectionIndex ? "upgrade_select" : "upgrade_down");
		}//Play some sort of sooound
		SoundManager.instance.playSound(selectionSound);
		options [selectionIndex].Activate();
		header.speed = 1;

		//Wait for everything to be over
		yield return new WaitForSeconds (1.75f);
		GameStateManager.instance.ChangeState(GameStateManager.GameStates.STATE_GAMEPLAY);
	}

	int selectionIndex;
	float moveCD, timePushing;

	public void RegisterInput ()
	{
		moveCD = moveCD > 0 ? moveCD - Time.deltaTime : 0;

		//Left + right selection
		float h = Input.GetAxis("Horizontal");

		if (Mathf.Abs(h) > .3f) {
			timePushing += Time.deltaTime;
			if (moveCD <= 0) {
				moveCD = timePushing > 3 ? 0.05f : 0.25f;
				selectionIndex += h > 0 ? 1 : -1;
				if (selectionIndex > options.Length - 1)
					selectionIndex = 0;
				if (selectionIndex < 0)
					selectionIndex = options.Length - 1;
				
				pointer.rectTransform.anchoredPosition = new Vector2 ((40 * selectionIndex) - (((options.Length - 1) * 40) / 2), 12);
				SoundManager.instance.playSound(switchSound);
			}
		}
		else
			timePushing = 0;
	}

	public void PauseUnpauseSpinning (bool pause)
	{
		foreach (Animator a in AllZones) {
			a.SetFloat("speed",pause ? 0 : 1);
		}
	}
}
