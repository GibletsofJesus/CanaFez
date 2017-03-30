using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
	public static GameTimer instance;
	public float timeElapsed, maxTime = 90;
	[SerializeField]
	TextMesh timerText;
	[SerializeField]
	flash timerFlash;

	void Start ()
	{
		instance = this;
	}

	void Update ()
	{
		if (GameStateManager.instance.GetState() == GameStateManager.GameStates.STATE_GAMEPLAY) {
			timeElapsed += timeElapsed < maxTime ? Time.deltaTime : 0;

			int seconds = (int)(maxTime - timeElapsed) % 60;
			int minutes = (int)(maxTime - timeElapsed) / 60;

			timerText.text = "" + minutes + (seconds < 10 ? ":0" + seconds : ":" + seconds);

			if (maxTime - timeElapsed <= 0) {
				GameStateManager.instance.ChangeState(GameStateManager.GameStates.STATE_GAMEOVER);
			}
		}
		if (maxTime - timeElapsed < 15f) {
			timerFlash.flashSpeed = (int)(maxTime - timeElapsed) / 2;
			timerFlash.enabled = true;
		}
	}
}
