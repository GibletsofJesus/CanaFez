using UnityEngine;

public class PauseState : GameState
{
	public override void OnStateActivate ()
	{
		Time.timeScale = 0.001f;
		GameStateManager.instance.PauseMenu.SetActive(true);
		GameStateManager.instance.GameplayUI.SetActive(false);
		menuIndex = 0;
	}

	public override void OnStateDeactivate ()
	{
		Time.timeScale = 1;
		GameStateManager.instance.PauseMenu.SetActive(false);
		GameStateManager.instance.GameplayUI.SetActive(true);
	}

	int menuIndex = 0;

	public override void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7))
			GameStateManager.instance.ChangeState(GameStateManager.instance.previousState);

		//UI menu

		#region axis based input
		/*
        if (Input.GetAxis("Pause") != 0 && !GameStateManager.instance.pausePressed)
        {
            GameStateManager.instance.pausePressed = true;
            GameStateManager.instance.ChangeState(GameStates.STATE_GAMEPLAY);
        }
        else if (Input.GetAxis("Pause")==0f)
        {
            GameStateManager.instance.pausePressed = false;
        }*/
		#endregion
	}
}
