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
	GameObject[] popups;
	[SerializeField]
	AudioClip[] blips;

	enum pauseMenu
	{
		main,
		rotation,
		sound,
	}


	float menuMoveCD, prevV, timePushing;

	public void Reset ()
	{
		state = pauseMenu.main;
		foreach (GameObject g in popups) {
			g.SetActive(false);
		}
		menuIndex = 0;
		arrow.rectTransform.anchoredPosition = new Vector2 (6, -10);
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
							menuIndex = menuIndex < 3 ? menuIndex + 1 : 0;
						}
						arrow.rectTransform.anchoredPosition = new Vector2 (6, -10 - (menuIndex * 9));
					}
					prevV = v;
					#endregion

					#region selection
					if (CrossPlatformInputManager.GetButtonDown("Jump")) {
						switch (menuIndex) {
							case 0:
								state = pauseMenu.rotation;
								popups [0].SetActive(true);
								break;
							case 1:
								float vol = SoundManager.instance.volumeMultiplayer;
								soundText.text = "" + Mathf.Round(vol * 100);
								soundBar.fillAmount = vol / 2;
								popups [1].SetActive(true);
								state = pauseMenu.sound;
								break;
							case 2:
								Time.timeScale = 1;
								SceneManager.LoadScene(0);
								break;
							case 3:
								//Are you sure window?
								Application.Quit();
								break;
						}
					}
					#endregion

					break;
				case pauseMenu.rotation:
					float h = Input.GetAxis("Horizontal");
					if (Mathf.Abs(h) > 0 && menuMoveCD == 0) {
						menuMoveCD = 0.5f;

						if (rotImg.rectTransform.localScale.x > 0) {
							SoundManager.instance.playSound(blips [2]);
							rotText.text = "CW";
							rotImg.rectTransform.localScale = new Vector3 (-1, 1, 1);
							PerspectiveChanger.instance.clockwise = true;
						}
						else {
							SoundManager.instance.playSound(blips [1]);
							rotText.text = "CCW";
							rotImg.rectTransform.localScale = Vector3.one;
							PerspectiveChanger.instance.clockwise = false;
						}
					}
					if (CrossPlatformInputManager.GetButtonDown("Jump")) {
						state = pauseMenu.main;
						popups [0].SetActive(false);
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

					if (CrossPlatformInputManager.GetButtonDown("Jump")) {
						state = pauseMenu.main;
						popups [1].SetActive(false);
					}
					break;
			}
		}

	}
}
