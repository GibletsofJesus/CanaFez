using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
	pauseMenu state;
	int menuIndex, rotIndex;
	[SerializeField]
	Image arrow, soundBar, rotImg;
	[SerializeField]
	Text soundText, rotText;
	[SerializeField]
	Text[] optionsText;
	[SerializeField]
	RectTransform[] popups;
	[SerializeField]
	GameObject[] popupUIElements;
	[SerializeField]
	AudioClip[] blips;

	float flashing;

	enum pauseMenu
	{
		main,
		transition,
		rotation,
		sound,
	}


	float menuMoveCD, prevV, timePushing;

	public void Reset ()
	{
		state = pauseMenu.main;

		menuIndex = 0;
		arrow.rectTransform.anchoredPosition = new Vector2 (6, -10);
		popupUIElements [0].SetActive(false);
		popupUIElements [1].SetActive(false);
		popups [0].sizeDelta = Vector2.zero;
		popups [1].sizeDelta = Vector2.zero;
	}

	void Update ()
	{
		if (GameStateManager.instance.GetState() == GameStateManager.GameStates.STATE_PAUSE) {

			menuMoveCD = menuMoveCD > 0 ? menuMoveCD - Time.unscaledDeltaTime : 0;

			switch (state) {
				case pauseMenu.main:
					#region up/down
					float v = Input.GetAxis("Vertical");
					if (Mathf.Abs(v) > 0 && menuMoveCD == 0) {

						if (v > 0) {
							SoundManager.instance.playSound(blips [0],1,0.75f + Mathf.Clamp(timePushing / 4,0,.5f) + Random.Range(-0.03f,0.03f));
							timePushing = prevV > 0 ? timePushing + 0.25f : 0;
							menuMoveCD = timePushing > 1 ? 0.05f : 0.15f;
							menuIndex = menuIndex > 0 ? menuIndex - 1 : 3;
						}
						else {
							SoundManager.instance.playSound(blips [0],1,0.75f + Mathf.Clamp(timePushing / 4,0,0.5f) + Random.Range(-0.03f,0.03f));
							timePushing = prevV < 0 ? timePushing + 0.25f : 0;
							menuMoveCD = timePushing > 1 ? 0.05f : 0.15f;
							menuIndex = menuIndex < 4 ? menuIndex + 1 : 0;
						}
						arrow.rectTransform.anchoredPosition = new Vector2 (5, -9 - (menuIndex * 9));
					}
					prevV = v;
					#endregion

					#region selection
					if (Input.GetButtonDown("Jump")) {
						switch (menuIndex) {
							case 0:
								Time.timeScale = 1;
								SceneManager.LoadScene(1);
								break;
							case 1:
								state = pauseMenu.transition;
								StartCoroutine(OpenBox(0,true));
								break;
							case 2:
								float vol = SoundManager.instance.volumeMultiplayer;
								soundText.text = "" + Mathf.Round(vol * 100);
								soundBar.fillAmount = vol / 2;
								StartCoroutine(OpenBox(1,true));
								state = pauseMenu.transition;
								break;
							case 3:
								Time.timeScale = 1;
								SceneManager.LoadScene(0);
								break;
							case 4:
								//Are you sure window?
								Application.Quit();
								break;
						}
					}
					#endregion

					#region flash selected text
					flashing++;
					if (flashing > 12)
						flashing = 0;

					foreach (Text t in optionsText) {
						t.color = new Color (.9f, .9f, .9f);
					}

					optionsText [menuIndex].color = new Color (flashing < 6 ? 0.9f : .6f, flashing < 6 ? .9f : .6f, flashing < 6 ? 0.9f : .6f);


					#endregion

					break;
				case pauseMenu.rotation:
					float h = Input.GetAxis("Horizontal");
					if (Mathf.Abs(h) > 0 && menuMoveCD == 0) {
						menuMoveCD = 0.25f;

						if (rotImg.rectTransform.localScale.x > 0) {
							//SoundManager.instance.playSound(blips [2]);
							rotText.text = "CW";
							rotImg.rectTransform.localScale = new Vector3 (-1.2f, 1.2f, 1);
							SoundManager.instance.playSound(blips [1]);
							PerspectiveChanger.instance.clockwise = true;
						}
						else {
							//SoundManager.instance.playSound(blips [1]);
							rotText.text = "CCW";
							rotImg.rectTransform.localScale = Vector3.one * 1.2f;
							SoundManager.instance.playSound(blips [2]);
							PerspectiveChanger.instance.clockwise = false;
						}
					}
					if (Input.GetButtonDown("Jump")) {
						state = pauseMenu.transition;
						StartCoroutine(OpenBox(0,false));
					}
					break;
				case pauseMenu.sound:
					float _h = Input.GetAxis("Horizontal");
					if (Mathf.Abs(_h) > 0) {						
						float vol = SoundManager.instance.volumeMultiplayer;
						vol += _h * Time.unscaledDeltaTime / 2;
						vol = Mathf.Clamp01(vol);
						soundBar.fillAmount = vol / 2;

						if ((!soundText.text.EndsWith("5") && !soundText.text.EndsWith("0")) && Mathf.Round(vol * 100) % 5 == 0)
							SoundManager.instance.playSound(blips [0]);
						soundText.text = "" + Mathf.Round(vol * 100);
						SoundManager.instance.changeVolume(vol);
					}

					if (Input.GetButtonDown("Jump")) {
						state = pauseMenu.transition;
						StartCoroutine(OpenBox(1,false));
					}
					break;
			}
		}
	}

	IEnumerator OpenBox (int index, bool open)
	{
		SoundManager.instance.playSound(blips [open ? 3 : 4],1,2);

		popupUIElements [index].SetActive(false);

		float lerpy = 0;
		while (lerpy < 1) {
			lerpy += Time.fixedUnscaledDeltaTime * 2;
			popups [index].sizeDelta = new Vector2 (
				Mathf.Lerp(0,index == 0 ? 36 : 34,(open ? lerpy : 1 - lerpy) * 2),
				Mathf.Lerp(4,index == 0 ? 23.5f : 25,((open ? lerpy : 1 - lerpy) * 2) - 1));
			yield return new WaitForEndOfFrame ();
		}
		if (open)
			state = index == 0 ? pauseMenu.rotation : pauseMenu.sound;
		else
			state = pauseMenu.main;
		popupUIElements [index].SetActive(open);
	}
}
