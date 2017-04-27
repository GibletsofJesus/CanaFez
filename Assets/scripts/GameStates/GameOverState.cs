using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverState : GameState
{
	public override void OnStateActivate ()
	{
		//GameStateManager.instance.GameOverUI.SetActive(true);
		GameStateManager.instance.GameplayUI.SetActive(false);
		EndGameUI.instance.StartCoroutine(EndGameUI.instance.EndGame());
	}

	public override void OnStateDeactivate ()
	{
		//
	}

	public override void Update ()
	{
	}
}