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

	state currentState = state.NotReady;

	enum state
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
		foreach (Sprite s in  minimapCapture.instance.captures) {
			minimapThing.sprite = s;
			SoundManager.instance.playSound(sounds [1],1,1.25f);

			//whilst this is happening, scroll up the final score thing
			//Also move the XP bar along

			yield return new WaitForSeconds (0.1f);
		}


		//NEW HIGHSCORE! if it is
		//Record that shit (but that's a job for another day)
		yield return StartCoroutine(highScores.openWindows(true));

		currentState = state.Ready;
		//Hand control over to the player
		Arrow.rectTransform.anchoredPosition = new Vector3 (-3.25f, 2.25f - (index * 1.5f), 0);
		minimapIndex = minimapCapture.instance.captures.Count - 1;
	}

	int index = 0, minimapIndex = 0;
	float moveCD = 0;

	void Update ()
	{
		if (currentState == state.Ready) {
			moveCD = moveCD > 0 ? moveCD - Time.deltaTime : 0;
			float v = Input.GetAxis("Vertical");
			if (Mathf.Abs(v) > 0.15f && moveCD <= 0) {
				index += v > 0 ? -1 : 1;
				if (index < 0)
					index = 3;
				if (index > 3)
					index = 0;

				moveCD = 0.5f;
				SoundManager.instance.playSound(sounds [1]);
				Arrow.rectTransform.anchoredPosition = new Vector3 (-29, 10 - (index * 1.5f), 0);
			}

			if (Input.GetButtonDown("Jump")) {
				switch (index) {
					case 0:
						SceneManager.LoadScene(1);
						break;
					case 1:
						//Open colour palette menu
						break;
					case 2:
						//Open leadboards
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