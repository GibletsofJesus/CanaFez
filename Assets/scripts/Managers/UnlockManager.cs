using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public int XPtoUnlock;
	public bool unlocked;
	public string name;
	public unlockType type;
	public Texture2D newPalette;
}

public class UnlockManager : MonoBehaviour
{
	public static UnlockManager instance;
	public int playerXP, playerLevel;

	public Unlockable[] LevelsToUnlock;

	void Start ()
	{
		instance = this;
		playerXP = PlayerPrefs.GetInt("PlayerXP");
	}


	void UpdateXP (int XPtoAdd)
	{
		PlayerPrefs.SetInt("PlayerXP",playerXP);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!instance)
			instance = this;
	}
}
