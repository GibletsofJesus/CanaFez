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
	Image MainMenuArrow;
	int menuIndex;
	[Header("Leaderboards")]
	[SerializeField]
	Image LeaderboardsImage;
	[SerializeField]
	Text LeaderboardRanks, LeaderboardNames, LeaderboardScores;

	MenuState state = MenuState.idle;

	enum MenuState
	{
		idle,
		start,
		leaderboards,
		main}
	;

	Vector3 defaultPos;

	// Use this for initialization
	void Start ()
	{
		defaultPos = Camera.main.transform.position;
	}

	public void AllowStart ()
	{
		state = MenuState.start;
	}

	void Update ()
	{
		//Melee type camera
		float v = Input.GetAxis("RSVertical");
		float h = Input.GetAxis("RSHorizontal");

		Camera.main.transform.localPosition = defaultPos +
		Camera.main.transform.right * h * 0.15f +
		Vector3.up * v * 0.15f;
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
					MainMenuArrow.gameObject.SetActive(true);
				}
				break;
			case MenuState.main:
				menuMoveCD = menuMoveCD > 0 ? menuMoveCD - Time.deltaTime : 0;

				#region menu up/down
				float v = CrossPlatformInputManager.GetAxis("Vertical");
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
					MainMenuArrow.rectTransform.anchoredPosition = new Vector3 (-198, -20 - (menuIndex * 40) - 0.15f, 15);
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
							LeaderboardsImage.gameObject.SetActive(true);
							StartCoroutine(Leaderboard(true));
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
			case MenuState.leaderboards:
				if (CrossPlatformInputManager.GetButton("Jump")) {
					state = MenuState.idle;
					StartCoroutine(Leaderboard(false));
					SoundManager.instance.playSound(blips [2],1,1);
				}
				break;
		}
	}

	IEnumerator Leaderboard (bool open)
	{
		if (!open) {
			LeaderboardNames.gameObject.SetActive(false);
			LeaderboardRanks.gameObject.SetActive(false);
			LeaderboardScores.gameObject.SetActive(false);
		}

		float lerpy = 0;
		while (lerpy < 1) {
			lerpy += Time.deltaTime;
			LeaderboardsImage.rectTransform.sizeDelta = new Vector2 (
				Mathf.Lerp(0,114,(open ? lerpy : 1 - lerpy) * 2),
				Mathf.Lerp(4,98,((open ? lerpy : 1 - lerpy) * 2) - 1));
			yield return new WaitForEndOfFrame ();
		}
		mainMenu [2].GetComponent<flash>().enabled = false;
		mainMenu [2].enabled = true;
		if (open) {
			LeaderboardRanks.gameObject.SetActive(true);
			LeaderboardNames.gameObject.SetActive(true);
			LeaderboardScores.gameObject.SetActive(true);

			char[] spliiterChar = { '\r', '\n' };

			//Do some mad shit with typing things out
			string[] ranks = LeaderboardRanks.text.Split(spliiterChar,10);
			string[] names = LeaderboardNames.text.Split(spliiterChar,10);
			string[] scores = LeaderboardScores.text.Split(spliiterChar,10);

			string _ranks = LeaderboardRanks.text;
			string _names = LeaderboardNames.text;
			string _scores = LeaderboardScores.text;

			LeaderboardScores.text = "";
			LeaderboardNames.text = "";
			LeaderboardRanks.text = "";

			#region method A
			/*for (int i = 0; i < 3; i++) {
				
				LeaderboardRanks.text = "";
				foreach (string s in ranks) {
					if (i < s.Length)
						LeaderboardRanks.text += s.Substring(0,i) + '\n';
					else
						LeaderboardRanks.text += s + '\n';
				}
				yield return new WaitForEndOfFrame ();
				yield return new WaitForEndOfFrame ();
				yield return new WaitForEndOfFrame ();
			}

			for (int i = 0; i < 7; i++) {
				
				LeaderboardNames.text = "";
				foreach (string s in names) {
					if (i < s.Length)
						LeaderboardNames.text += s.Substring(0,i) + '\n';
					else
						LeaderboardNames.text += s + '\n';
				}

				yield return new WaitForEndOfFrame ();
				yield return new WaitForEndOfFrame ();
				yield return new WaitForEndOfFrame ();
			}

			for (int i = 0; i < 10; i++) {
				LeaderboardScores.text = "";
				foreach (string s in scores) {
					if (i < s.Length)
						LeaderboardScores.text += s.Substring(0,i) + '\n';
					else
						LeaderboardScores.text += s + '\n';
				}
				yield return new WaitForEndOfFrame ();
				yield return new WaitForEndOfFrame ();
				yield return new WaitForEndOfFrame ();
			}*/
			#endregion

			#region method B
			for (int i = 0; i < _ranks.Length; i++) {
				LeaderboardRanks.text += _ranks [i];
				yield return new WaitForEndOfFrame ();
			}
			for (int i = 0; i < _names.Length; i++) {
				LeaderboardNames.text += _names [i];
				yield return new WaitForEndOfFrame ();
			}
			for (int i = 0; i < _scores.Length; i++) {
				LeaderboardScores.text += _scores [i];
				yield return new WaitForEndOfFrame ();
			}
			#endregion

			#region method C
			//Same as method B, only do a line at a time
			for (int i = 0; i < _ranks.Length; i++) {
				LeaderboardRanks.text += _ranks [i];
				yield return new WaitForEndOfFrame ();
			}
			for (int i = 0; i < _names.Length; i++) {
				LeaderboardNames.text += _names [i];
				yield return new WaitForEndOfFrame ();
			}
			for (int i = 0; i < _scores.Length; i++) {
				LeaderboardScores.text += _scores [i];
				yield return new WaitForEndOfFrame ();
			}
			#endregion
		}
		state = open ? MenuState.leaderboards : MenuState.main;
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
