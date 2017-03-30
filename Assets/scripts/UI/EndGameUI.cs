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
	Text Header, completionNum, cityText;
	[SerializeField]
	Text[] options;
	[SerializeField]
	GameObject[] boxes;
	[SerializeField]
	SpriteRenderer minimapThing;
	[SerializeField]
	Image background, Arrow;
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

	public IEnumerator test ()
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

		boxes [2].SetActive(true);
		SoundManager.instance.playSound(sounds [1]);

		//scroll through game progress
		yield return new WaitForSeconds (0.5f);
		foreach (Sprite s in  minimapCapture.instance.captures) {
			minimapThing.sprite = s;
			SoundManager.instance.playSound(sounds [1],1,1.25f);
			yield return new WaitForSeconds (0.1f);
		}

		//short pause for dramatic effect
		yield return new WaitForSeconds (2);
		SoundManager.instance.playSound(sounds [0]);
		//Reveal completeion %
		boxes [1].SetActive(true);

		completionNum.text = WorldGen.instance.GetCompletionRating() + "%";
		cityText.text = "Of " + WorldGen.instance.GetCurrentCityName() + " explored";

		//NEW HIGHSCORE! if it is
		//Record that shit (but that's a job for another day)

		currentState = state.Ready;
		boxes [0].SetActive(true);
		Arrow.rectTransform.anchoredPosition = new Vector3 (-29, 10 - (index * 20), 0);
		minimapIndex = minimapCapture.instance.captures.Count - 1;
	}

	int index = 0, minimapIndex = 0;
	float moveCD = 0;

	void Update ()
	{
		if (currentState == state.Ready) {
			moveCD = moveCD > 0 ? moveCD - Time.deltaTime : 0;
			float v = CrossPlatformInputManager.GetAxis("Vertical");
			if (Mathf.Abs(v) > 0 && moveCD <= 0) {
				/*index += v > 0 ? -1 : 1;
				if (index < 0)
					index = options.Length - 1;
				if (index > options.Length - 1)
					index = 0;*/

				index = index == 0 ? 1 : 0;

				moveCD = 0.5f;
				SoundManager.instance.playSound(sounds [1]);
				Arrow.rectTransform.anchoredPosition = new Vector3 (-29, 10 - (index * 20), 0);
			}


			float h = CrossPlatformInputManager.GetAxis("Horizontal");
			if (Mathf.Abs(h) > 0 && moveCD <= 0) {
				if (h > 0) {
					minimapIndex++;
					if (minimapIndex > minimapCapture.instance.captures.Count - 1)
						minimapIndex = minimapCapture.instance.captures.Count - 1;
				}
				else {
					minimapIndex--;
					if (minimapIndex < 0)
						minimapIndex = 0;
				}
				SoundManager.instance.playSound(sounds [1],1,1.25f);
				moveCD = 0.1f;
				minimapThing.sprite = minimapCapture.instance.captures [minimapIndex];
			}
		

			if (CrossPlatformInputManager.GetButtonDown("Jump")) {
				switch (index) {
					case 0:
						SceneManager.LoadScene(1);
						break;
					case 1:
						SceneManager.LoadScene(0);
						break;
				}
			}


			//handle input n shit
		}
	}
}