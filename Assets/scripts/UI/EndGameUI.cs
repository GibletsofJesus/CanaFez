using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

public class EndGameUI : MonoBehaviour
{
	public static EndGameUI instance;

	//We're going to need a few references to say the least
	[SerializeField]
	Text Header;
	[SerializeField]
	Image background, Arrow, minimapThing;
	[SerializeField]
	EnterHighscore highScores;
	[SerializeField]
	GameObject mostOfTheUI, UIOptions;
	[SerializeField]
	Color[] backgroundLerpColours;
	[SerializeField]
	AudioClip[] sounds;

	[SerializeField]
	Text finalScoreDisplay, finalScoreHighscoreFlash;

	public state currentState = state.NotReady;

	public enum state
	{
		NotReady,
		Ready
	}

	void Start ()
	{
		instance = this;
	}

	public IEnumerator EndGame ()
	{
		float lerpy	= 0;
		//Lerp in the background
		PlayerCharacter.instance.Pause();
		//Kill the jump charge sound
		SoundManager.instance.managedAudioSources [0].AudioSrc.volume = 0;
		while (lerpy < 1) {
			lerpy += Time.deltaTime;
			//Fade out ground ambience
			SoundManager.instance.managedAudioSources [2].volumeLimit = Mathf.Lerp(SoundManager.instance.volumeMultiplayer,0,lerpy);
			SoundManager.instance.managedAudioSources [2].AudioSrc.volume = SoundManager.instance.managedAudioSources [2].volumeLimit;
			background.color = Color.Lerp(backgroundLerpColours [0],backgroundLerpColours [1],lerpy);
			yield return new WaitForEndOfFrame ();		
		}

		yield return new WaitForSeconds (1);
		SoundManager.instance.playSound(sounds [0]);
		Header.enabled = true;
		yield return new WaitForSeconds (1);

		//Enable a the rest of the UI, minus the retry + quit options
		mostOfTheUI.SetActive(true);
		SoundManager.instance.playSound(sounds [1]);

		//scroll through game progress
		yield return new WaitForSeconds (0.5f);
		foreach (KeyValuePair<Sprite,int> s in  minimapCapture.instance.captures) {
			minimapThing.sprite = s.Key;
			SoundManager.instance.playSound(sounds [1],1,1.25f);

			//Also move the XP bar along

			finalScoreDisplay.text = "";
			UIScoreManager.instance.SpawnEndGameText(s.Value);

			//if it's a HIIIGHSCORe
			scoreToAdd += s.Value;
			if (displayScore > int.Parse(EnterHighscore.instance.scores [9].Value) && !finalScoreHighscoreFlash.gameObject.activeSelf) {
				SoundManager.instance.playSound(sounds [2]);
				finalScoreHighscoreFlash.gameObject.SetActive(true);
			}
			yield return new WaitForSeconds (0.1f);
		}

		while (scoreToAdd > 0) {
			if (UnlockManager.instance.experienceSlider.value == UnlockManager.instance.experienceSlider.maxValue && UnlockManager.instance.playerLevel < 10) {
				//HOOOO BOY AN UNLOCK HAPPENING
				//do things
				yield return StartCoroutine(Unlock(false));
				UnlockManager.instance.NextLevel();
				if (UnlockManager.instance.playerLevel > 9) {
					yield return StartCoroutine(Unlock(true));
				}
			}
			yield return new WaitForEndOfFrame ();
		}

		yield return new WaitForSeconds (2);

		//Record high score
		//If current points are higher than last position in leaderboard
		if (UIScoreManager.instance.points > int.Parse(EnterHighscore.instance.scores [9].Value))
			yield return StartCoroutine(highScores.openWindows(true));

		finalScoreHighscoreFlash.gameObject.SetActive(false);

		currentState = state.Ready;
		//Hand control over to the player
		minimapIndex = minimapCapture.instance.captures.Count - 1;
		UIOptions.SetActive(true);
	}

	[Header("Unlock popup refs")]
	[SerializeField]
	Text UnlockHeader, UnlockText;
	[SerializeField]
	RectTransform UnlockBox;

	IEnumerator Unlock (bool special)
	{
		float lerpy = 0;
		SoundManager.instance.playSound(sounds [3]);
		while (lerpy < 1) {
			lerpy += Time.deltaTime;
			UnlockBox.sizeDelta = new Vector2 (
				Mathf.Lerp(0,230,lerpy * 2),
				Mathf.Lerp(4,52,(lerpy * 2) - 1));
			yield return new WaitForEndOfFrame ();
		}

		UnlockHeader.gameObject.SetActive(true);
		if (special)
			foreach (Text t in  UnlockHeader.GetComponentsInChildren<Text>())
				t.text = "CUSTOM PALETTE" + '\n' + "MAKER UNLOCKED!!";
		else
			UnlockText.text = UnlockManager.instance.LevelsToUnlock [UnlockManager.instance.playerLevel].name;
		
		while (!Input.GetButtonDown("Jump"))
			yield return null;

		lerpy = 0;
		UnlockHeader.gameObject.SetActive(false);
		UnlockText.text = "";
		SoundManager.instance.playSound(sounds [4]);
		while (lerpy < 1) {
			lerpy += Time.deltaTime;
			UnlockBox.sizeDelta = new Vector2 (
				Mathf.Lerp(230,0,lerpy * 2),
				Mathf.Lerp(52,4,((1 - lerpy) * 2) - 1));
			yield return new WaitForEndOfFrame ();
		}
		
	}

	int index = 0, minimapIndex = 0;
	float moveCD = 0;
	int scoreToAdd = 0, displayScore = 0;

	void Update ()
	{
		if (scoreToAdd > 75) {
			displayScore += 75;
			scoreToAdd -= 75;
			UnlockManager.instance.playerXP += 75;
		}
		else if (scoreToAdd > 0) {
				displayScore += scoreToAdd;
				UnlockManager.instance.playerXP += scoreToAdd;
				scoreToAdd = 0;
			}
		finalScoreDisplay.text = string.Format("{0:n0}",displayScore);

		if (currentState == state.Ready) {
			moveCD = moveCD > 0 ? moveCD - Time.deltaTime : 0;
			float v = Input.GetAxis("Vertical");
			if (Mathf.Abs(v) > 0.15f && moveCD <= 0) {
				index += v > 0 ? -1 : 1;
				if (index < 0)
					index = 3;
				if (index > 3)
					index = 0;

				moveCD = 0.25f;
				SoundManager.instance.playSound(sounds [1]);
				Arrow.rectTransform.anchoredPosition = new Vector3 (-3.25f, 2.25f - (index * 1.5f), 0);
			}

			if (Input.GetButtonDown("Jump")) {
				switch (index) {
					case 0:
						SceneManager.LoadScene(1);
						break;
					case 1:
						//Open colour palette menu
						currentState = state.NotReady;
						StartCoroutine(CustomPaletteCreator.instance.OpenCloseWindow(true));
						break;
					case 2:
						//Open leadboards
						currentState = state.NotReady;
						StartCoroutine(EnterHighscore.instance.openWindows(false));
						break;
					case 3:
						SceneManager.LoadScene(0);
						break;
				}
			}


			//handle input n shit
		}
	}
}