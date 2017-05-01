using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Unlockable
{
	public enum unlockType
	{
		paletteCreator,
		newPalette,
		newUpgrade
		//Maaaybe?
	}

	public string name;
	public int XPtoUnlock;
	public bool unlocked;
	public unlockType type;
	public Texture2D newPalette;
}

public class UnlockManager : MonoBehaviour
{
	public static UnlockManager instance;
	public int playerXP, playerLevel;

	public Slider experienceSlider;
	public 	Text experienceNumber;

	public Unlockable[] LevelsToUnlock;

	void Start ()
	{
		instance = this;
		playerXP = PlayerPrefs.GetInt("PlayerXP");

		bool first = true;

		if (experienceSlider) {
			for (int i = 0; i < LevelsToUnlock.Length; i++) {
				if (playerXP < LevelsToUnlock [i].XPtoUnlock) {
					experienceSlider.minValue = i > 0 ? LevelsToUnlock [i - 1].XPtoUnlock : 0;
					experienceSlider.maxValue = LevelsToUnlock [i].XPtoUnlock;
					break;
				}
			}
			experienceSlider.value = playerXP;
		}
		for (int i = 0; i < LevelsToUnlock.Length; i++) {
			if (playerXP > LevelsToUnlock [i].XPtoUnlock) {
				playerLevel++;
			}
		}
		//Now we've done this, ever new upgrade that we pass the threshold of unlocking, display the unlock sequence (thing)
	}

	public void NextLevel ()
	{
		for (int i = 0; i < LevelsToUnlock.Length; i++) {
			playerLevel++;
			experienceSlider.minValue = i > 0 ? LevelsToUnlock [i - 1].XPtoUnlock : 0;
			experienceSlider.minValue = LevelsToUnlock [i].XPtoUnlock;
			break;
		}
	}

	public void UpdateExperienceText ()
	{
		experienceNumber.text = string.Format("{0:n0}",playerXP - experienceSlider.minValue) + "/" + string.Format("{0:n0}",experienceSlider.maxValue);
	}

	void UpdateXP ()
	{
		PlayerPrefs.SetInt("PlayerXP",playerXP);
	}
	
	// Update is called once per frame
	void Update ()
	{
		//CHANGE ME
		if (experienceSlider) {
			if (experienceSlider.gameObject.activeInHierarchy) {
				UpdateXP();
				experienceSlider.value = playerXP;
				UpdateExperienceText();
			}
		}
		if (!instance)
			instance = this;
	}
}
