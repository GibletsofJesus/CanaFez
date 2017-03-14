using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;

public class splashScreenControls : MonoBehaviour
{

	[SerializeField]
	AudioClip[] blips;
	[SerializeField]
	Animator _anim;
	[SerializeField]
	Text[] mainMenu;
	[SerializeField]
	flash startPrompt;
	[SerializeField]
	Image Arrow;
	int menuIndex;

	MenuState state = MenuState.idle;

	enum MenuState
	{
		idle,
		start,
		main}
	;

	// Use this for initialization
	void Start ()
	{
		
	}

	public void AllowStart ()
	{
		state = MenuState.start;
	}

	// Update is called once per frame
	void LateUpdate ()
	{
		switch (state) {
			case MenuState.start:
				if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7)) {
					startPrompt.enabled = false;
					startPrompt.GetComponent<SpriteRenderer>().enabled = false;
					mainMenu [0].gameObject.SetActive(true);
					state = MenuState.main;
					Arrow.gameObject.SetActive(true);
				}
				break;
			case MenuState.main:
				menuMoveCD = menuMoveCD > 0 ? menuMoveCD - Time.deltaTime : 0;

				#region menu up/down
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
					Arrow.rectTransform.anchoredPosition = new Vector2 (-90, -28 - (menuIndex * 15) - 0.2f);
				}
				prevV = v;
				#endregion

				#region selecting menu items
				if (CrossPlatformInputManager.GetButton("Jump")) {
					state = MenuState.idle;
					switch (menuIndex) {
						case 0:
							mainMenu [0].GetComponent<flash>().enabled = true;
							SoundManager.instance.playSound(blips [1],1,1);
							StartCoroutine(StartGame());
							//Start da video game
							break;
						case 1:
							mainMenu [1].GetComponent<flash>().enabled = true;
							SoundManager.instance.playSound(blips [1],1,1);
							//go into options menu
							break;
						case 2:
							mainMenu [2].GetComponent<flash>().enabled = true;
							SoundManager.instance.playSound(blips [1],1,1);
							//Show high scores table
							break;
						case 3:
							mainMenu [3].GetComponent<flash>().enabled = true;
							SoundManager.instance.playSound(blips [1],1,1);
							Application.Quit();
							break;
					}
				}
				#endregion

				break;
		}
	}

	IEnumerator StartGame ()
	{
		yield return new WaitForSeconds (.5f);
		_anim.Play("splash_outro");
		yield return new WaitForSeconds (1);
		SceneManager.LoadScene(1);
	}

	float menuMoveCD, prevV, timePushing;

}
