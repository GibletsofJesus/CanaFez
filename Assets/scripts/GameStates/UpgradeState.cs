using UnityEngine;

public class UpgradeState : GameState
{
	float prevLerpSpeed;

	public override void OnStateActivate ()
	{
		UpgradeManager.instance.StartCoroutine(UpgradeManager.instance.SetupUI());
		prevLerpSpeed = PerspectiveChanger.instance.lerpSpeed;
		PerspectiveChanger.instance.lerpSpeed = 0;
		UpgradeManager.instance.upgradeCanvas.SetActive(true);
		UpgradeManager.instance.PauseUnpauseSpinning(true);
		PlayerCharacter.instance.Pause();
		GameStateManager.instance.UpgradeUI.SetActive(true);
		GameStateManager.instance.GameplayUI.SetActive(false);
		PerspectiveChanger.instance.lerpSpeed = 0;
	}

	public override void OnStateDeactivate ()
	{
		PerspectiveChanger.instance.lerpSpeed = prevLerpSpeed;
		UpgradeManager.instance.upgradeCanvas.SetActive(true);
		PlayerCharacter.instance.UnPause();
		UpgradeManager.instance.PauseUnpauseSpinning(false);
		GameStateManager.instance.UpgradeUI.SetActive(false);
		GameStateManager.instance.GameplayUI.SetActive(true);
	}

	public override void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Escape)) {
			GameStateManager.instance.ChangeState(GameStateManager.GameStates.STATE_GAMEPLAY);
		}
	}
}